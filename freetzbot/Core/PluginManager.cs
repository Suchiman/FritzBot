using FritzBot.DataModel;
using FritzBot.Plugins;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FritzBot.Core
{
    public static class PluginManager
    {
        private const string DefaultReferences = "mscorlib.dll,System.dll,System.Core.dll,System.Configuration.dll,System.IO.Compression.dll,System.Web.dll,System.Xml.dll,System.Xml.Linq.dll,System.Net.FtpClient.dll,CsQuery.dll,EntityFramework.dll,Newtonsoft.Json.dll,Irc4netButSmarter.dll";

        private static readonly Dictionary<string, PluginInfo> _lookupDictionary = new Dictionary<string, PluginInfo>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<PluginInfo> _plugins = new List<PluginInfo>();

        public static IEnumerable<PluginInfo> Plugins => _plugins;

        /// <summary>
        /// Fährt den PluginManager herunter
        /// </summary>
        public static void Shutdown()
        {
            _plugins.Where(x => x.IsBackgroundTask).TryLogEach(x => x.Stop());
        }

        /// <summary>
        /// Instanziert die Typen, entfernt bereits vorhandene Typen mit gleichem FullName und fügt die neuen hinzu.
        /// </summary>
        public static int AddDistinct(bool AutostartTask, params Type[] Types)
        {
            IEnumerable<Type> FilteredTypes = Types.Where(x => !x.IsAbstract && !x.IsInterface && typeof(PluginBase).IsAssignableFrom(x));
            Remove(x => FilteredTypes.Select(y => y.FullName).Contains(x.GetType().FullName));

            List<PluginInfo> NewPlugins = FilteredTypes.Select(x => new PluginInfo(x)).ToList();
            if (AutostartTask)
            {
                NewPlugins.Where(x => x.IsBackgroundTask).TryLogEach(x => x.Start());
            }
            _plugins.AddRange(NewPlugins);

            foreach (PluginInfo plugin in NewPlugins)
            {
                foreach (string name in plugin.Names)
                {
                    _lookupDictionary[name] = plugin;
                }
            }

            return NewPlugins.Count;
        }

        /// <summary>
        /// Entfernt alle ICommands und IBackgroundTasks auf die die Bedingung zutrifft.
        /// </summary>
        public static int Remove(Func<PluginInfo, bool> bedingung)
        {
            List<PluginInfo> toremove = _plugins.Where(bedingung).ToList();
            toremove.Where(x => x.IsBackgroundTask).TryLogEach(x => x.Stop());
            _plugins.RemoveAll(x => toremove.Contains(x));

            foreach (string Name in toremove.SelectMany(x => x.Names))
            {
                _lookupDictionary.Remove(Name);
            }

            return toremove.Count;
        }

        /// <summary>
        /// Gibt das erste PluginInfo zurück mit dem angegebenen Namen
        /// </summary>
        public static PluginInfo? Get(string name)
        {
            if (_lookupDictionary.TryGetValue(name, out PluginInfo? plugin))
            {
                return plugin;
            }
            return null;
        }

        /// <summary>
        /// Gibt die PluginInfos zurück mit dem angegebenen Plugin Typ
        /// </summary>
        public static IEnumerable<PluginInfo> GetOfType<T>()
        {
            return Plugins.Where(x => x.Plugin is T);
        }

        /// <summary>
        /// Initialisiert das Plugin System Asynchron
        /// </summary>
        public static void BeginInit(bool AutostartTask)
        {
            ThreadPool.QueueUserWorkItem(x => { Init(AutostartTask); });
        }

        /// <summary>
        /// Initialisiert das Plugin System und instanziert alle Plugins
        /// </summary>
        public static void Init(bool AutostartTask)
        {
            string PluginDirectory = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location)!, "plugins");
            if (!Directory.Exists(PluginDirectory))
            {
                Directory.CreateDirectory(PluginDirectory);
            }
            List<Type> allTypes = new List<Type>();
            string[] allFiles = Directory.GetFiles(PluginDirectory).Where(x => x.EndsWith(".cs")).ToArray<string>();
            Assembly Bot = Assembly.GetExecutingAssembly();
            if (allFiles.Length > 0)
            {
                try
                {
                    Assembly Compiled = LoadSource(allFiles);
                    allTypes.AddRange(Compiled.GetTypes());
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Das Laden der Source Module ist fehlgeschlagen und werden deshalb nicht zur Verfügung stehen!");
                }
            }
            allTypes.AddRange(Bot.GetTypes().Where(x => !allTypes.Contains(x, y => y.FullName!)));
            AddDistinct(AutostartTask, allTypes.ToArray<Type>()); //Die Methode verwirft alle Typen die nicht von PluginBase abgeleitet sind
        }

        public static void RecycleScoped(PluginBase plugin)
        {
            PluginInfo info = _plugins.FirstOrDefault(x => x.Id == plugin.PluginId);
            info.Recycle(plugin);
        }

        /// <summary>
        /// Lädt ein oder mehrere Plugins aus den gegebenen Dateien und initialisiert sie
        /// </summary>
        /// <param name="Path"></param>
        public static int LoadPluginFromFile(params string[] Path)
        {
            Assembly assembly = LoadSource(Path);
            return AddDistinct(true, assembly.GetTypes());
        }

        /// <summary>
        /// Lädt ein Plugin mit angegebenen Namen aus der gegebenen Assembly
        /// </summary>
        /// <param name="assembly">Die Assembly die den Typ beinhaltet</param>
        /// <param name="name">Der Name des Types</param>
        public static int LoadPluginByName(Assembly assembly, string name)
        {
            foreach (Type t in assembly.GetTypes())
            {
                if (t.GetCustomAttribute<NameAttribute>()?.Names?.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    return AddDistinct(true, t);
                }
            }
            return 0;
        }

        /// <summary>
        /// Kompiliert Quellcode im Arbeitsspeicher zu einem Assembly
        /// </summary>
        /// <param name="fileName">Ein Array das die Dateinamen enthält</param>
        /// <returns>Das aus den Quellcode erstellte Assembly</returns>
        public static Assembly LoadSource(params string[] fileName)
        {
            string[] assemblies = ConfigHelper.GetString("ReferencedAssemblies", DefaultReferences).Split(',');
            string FrameworkAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            string BotAssemblyPath = Path.GetDirectoryName(typeof(PluginManager).Assembly.Location)!;

            var references = new List<MetadataReference>();
            references.Add(MetadataReference.CreateFromFile(typeof(PluginManager).Assembly.Location));
            foreach (string assembly in assemblies)
            {
                string path = "";
                if (File.Exists((path = Path.Combine(BotAssemblyPath, assembly))) || File.Exists((path = Path.Combine(FrameworkAssemblyPath, assembly))))
                {
                    references.Add(MetadataReference.CreateFromFile(path));
                }
            }

            var syntaxTrees = new List<SyntaxTree>();
            foreach (string file in fileName)
            {
                using (var stream = File.OpenRead(file))
                {
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(SourceText.From(stream)));
                }
            }

            var compilation = CSharpCompilation.Create("FritzBotPlugins")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTrees);

            ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

            bool error = false;
            foreach (Diagnostic diag in diagnostics)
            {
                switch (diag.Severity)
                {
                    case DiagnosticSeverity.Info:
                        Log.Information(diag.ToString());
                        break;
                    case DiagnosticSeverity.Warning:
                        Log.Warning(diag.ToString());
                        break;
                    case DiagnosticSeverity.Error:
                        error = true;
                        Log.Error(diag.ToString());
                        break;
                }
            }

            if (error)
            {
                throw new Exception("Compilation failed");
            }

            using (var outputAssembly = new MemoryStream())
            {
                compilation.Emit(outputAssembly);

                return Assembly.Load(outputAssembly.ToArray());
            }
        }
    }

    public class PluginInfo : IBackgroundTask, ICommand
    {
        public Type PluginType { get; }
        public PluginBase Plugin { get; }

        private readonly Dictionary<string, PluginBase> UserScoped = new Dictionary<string, PluginBase>();
        private readonly Dictionary<string, PluginBase> ChannelScoped = new Dictionary<string, PluginBase>();
        private readonly Dictionary<KeyValuePair<string, string>, PluginBase> UserChannelScoped = new Dictionary<KeyValuePair<string, string>, PluginBase>();

        public string Id { get; }
        public List<string> Names { get; }

        public bool AuthenticationRequired { get; }
        public bool ParameterRequired { get; }
        public bool ParameterRequiredSpecified { get; }

        public bool IsHidden { get; }
        public bool IsCommand { get; }
        public bool IsSubscribeable { get; }
        public bool IsBackgroundTask { get; }

        public string? HelpText { get; }
        public Scope InstanceScope { get; }

        public PluginInfo(Type plugin)
        {
            PluginType = plugin;
            Plugin = (PluginBase)Activator.CreateInstance(PluginType)!;
            Id = Plugin.PluginId;

            IsCommand = Plugin is ICommand;
            IsBackgroundTask = Plugin is IBackgroundTask;

            AuthenticationRequired = plugin.GetCustomAttribute<AuthorizeAttribute>() != null;

            HelpText = plugin.GetCustomAttribute<HelpAttribute>()?.Help;

            IsHidden = plugin.GetCustomAttribute<HiddenAttribute>() != null;

            Names = plugin.GetCustomAttribute<NameAttribute>()?.Names.ToList() ?? new List<string>();
            if (plugin.GetCustomAttribute<ParameterRequiredAttribute>() is { } paramAtt)
            {
                ParameterRequiredSpecified = true;
                ParameterRequired = paramAtt.Required;
            }

            InstanceScope = plugin.GetCustomAttribute<ScopeAttribute>()?.Scope ?? Scope.Global;
            IsSubscribeable = plugin.GetCustomAttribute<SubscribeableAttribute>() != null;
        }

        public T GetScoped<T>(string channel, string user) where T : class
        {
            PluginBase plugin;
            switch (InstanceScope)
            {
                case Scope.Channel:
                    plugin = GetChannelScoped(channel);
                    break;
                case Scope.User:
                    plugin = GetUserScoped(user);
                    break;
                case Scope.UserChannel:
                    plugin = GetUserChannelScoped(user, channel);
                    break;
                case Scope.Global:
                default:
                    plugin = Plugin;
                    break;
            }
            return (T)(object)plugin;
        }

        public PluginBase GetUserScoped(string user)
        {
            if (!UserScoped.TryGetValue(user, out var pluginBase))
            {
                pluginBase = (PluginBase)Activator.CreateInstance(PluginType)!;
                UserScoped[user] = pluginBase;
            }
            return pluginBase;
        }

        public PluginBase GetUserChannelScoped(string user, string channel)
        {
            KeyValuePair<string, string> key = new KeyValuePair<string, string>(user, channel);
            if (!UserChannelScoped.TryGetValue(key, out var pluginBase))
            {
                pluginBase = (PluginBase)Activator.CreateInstance(PluginType)!;
                UserChannelScoped[key] = pluginBase;
            }
            return pluginBase;
        }

        public PluginBase GetChannelScoped(string channel)
        {
            if (!ChannelScoped.TryGetValue(channel, out var pluginBase))
            {
                pluginBase = (PluginBase)Activator.CreateInstance(PluginType)!;
                ChannelScoped[channel] = pluginBase;
            }
            return pluginBase;
        }

        public bool IsNamed(string name)
        {
            return Names.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        public void Start()
        {
            if (!IsBackgroundTask)
            {
                throw new NotSupportedException();
            }
            ((IBackgroundTask)Plugin).Start();
        }

        public void Stop()
        {
            if (!IsBackgroundTask)
            {
                throw new NotSupportedException();
            }
            ((IBackgroundTask)Plugin).Stop();
        }

        public void Run(IrcMessage theMessage)
        {
            if (!IsCommand)
            {
                throw new NotSupportedException();
            }
            ((ICommand)Plugin).Run(theMessage);
        }

        public void Recycle(PluginBase plugin)
        {
            foreach (KeyValuePair<string, PluginBase> instance in ChannelScoped.Where(x => x.Value == plugin).ToList())
            {
                ChannelScoped.Remove(instance.Key);
            }
            foreach (KeyValuePair<string, PluginBase> instance in UserScoped.Where(x => x.Value == plugin).ToList())
            {
                UserScoped.Remove(instance.Key);
            }
            foreach (KeyValuePair<KeyValuePair<string, string>, PluginBase> instance in UserChannelScoped.Where(x => x.Value == plugin).ToList())
            {
                UserChannelScoped.Remove(instance.Key);
            }
        }
    }
}