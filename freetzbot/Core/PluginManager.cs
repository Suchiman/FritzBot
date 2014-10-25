using FritzBot.DataModel;
using FritzBot.Plugins;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace FritzBot.Core
{
    public static class PluginManager
    {
        private const string DefaultReferences = "mscorlib.dll,System.dll,System.Core.dll,System.Configuration.dll,System.IO.Compression.dll,System.Web.dll,System.Xml.dll,System.Xml.Linq.dll,System.Net.FtpClient.dll,CsQuery.dll,EntityFramework.dll,Newtonsoft.Json.dll,Meebey.SmartIrc4net.dll";

        private static Dictionary<string, PluginInfo> _lookupDictionary = new Dictionary<string, PluginInfo>(StringComparer.OrdinalIgnoreCase);
        private static List<PluginInfo> _plugins = new List<PluginInfo>();

        public static IEnumerable<PluginInfo> Plugins { get { return _plugins; } }

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
        public static PluginInfo Get(string name)
        {
            PluginInfo plugin;
            if (_lookupDictionary.TryGetValue(name, out plugin))
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
            string PluginDirectory = Path.Combine(Environment.CurrentDirectory, "plugins");
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
                catch
                {
                    toolbox.Logging("Das Laden der Source Module ist fehlgeschlagen und werden deshalb nicht zur Verfügung stehen!");
                }
            }
            allTypes.AddRange(Bot.GetTypes().Where(x => !allTypes.Contains(x, y => y.FullName)));
            AddDistinct(AutostartTask, allTypes.ToArray<Type>()); //Die Methode verwirft alle Typen die nicht von PluginBase abgeleitet sind
        }

        public static void RecycleScoped(PluginBase plugin)
        {
            PluginInfo info = _plugins.FirstOrDefault(x => x.ID == plugin.PluginID);
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
                NameAttribute att = toolbox.GetAttribute<NameAttribute>(t);
                if (att != null && att.Names != null)
                {
                    if (att.Names.Any(x => x.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    {
                        return AddDistinct(true, t);
                    }
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
            ProjectId projectId = ProjectId.CreateNewId();

            string[] assemblies = ConfigHelper.GetString("ReferencedAssemblies", DefaultReferences).Split(',');
            string FrameworkAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            string BotAssemblyPath = Path.GetDirectoryName(typeof(PluginManager).Assembly.Location);

            Solution solution = new CustomWorkspace().CurrentSolution
                .AddProject(projectId, "FritzBotPlugins", "FritzBotPlugins", LanguageNames.CSharp)
                .AddMetadataReference(projectId, new MetadataFileReference(typeof(PluginManager).Assembly.Location, MetadataImageKind.Assembly))
                .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .WithProjectParseOptions(projectId, new CSharpParseOptions(LanguageVersion.Experimental));

            foreach (string assembly in assemblies)
            {
                string path = "";
                if (File.Exists((path = Path.Combine(BotAssemblyPath, assembly))) || File.Exists((path = Path.Combine(FrameworkAssemblyPath, assembly))))
                {
                    solution = solution.AddMetadataReference(projectId, new MetadataFileReference(path, MetadataImageKind.Assembly));
                }
            }

            foreach (string file in fileName)
            {
                solution = solution.AddDocument(DocumentId.CreateNewId(projectId), Path.GetFileName(file), new FileTextLoader(Path.GetFullPath(file), Encoding.UTF8));
            }

            Compilation compile = solution.GetProject(projectId).GetCompilationAsync().Result;
            ImmutableArray<Diagnostic> diagnostics = compile.GetDiagnostics();

            bool error = false;
            foreach (Diagnostic diag in diagnostics)
            {
                error |= diag.Severity == DiagnosticSeverity.Error;
                toolbox.Logging(diag.ToString());
            }

            if (error)
            {
                throw new Exception("Compilation failed");
            }

            using (var outputAssembly = new MemoryStream())
            using (var outputPdb = new MemoryStream())
            {
                compile.Emit(peStream: outputAssembly, pdbStream: outputPdb);

                return Assembly.Load(outputAssembly.ToArray(), outputPdb.ToArray());
            }
        }
    }

    public class PluginInfo : IBackgroundTask, ICommand
    {
        public Type PluginType { get; protected set; }
        public PluginBase Plugin { get; protected set; }

        private Dictionary<string, PluginBase> UserScoped = new Dictionary<string, PluginBase>();
        private Dictionary<string, PluginBase> ChannelScoped = new Dictionary<string, PluginBase>();
        private Dictionary<KeyValuePair<string, string>, PluginBase> UserChannelScoped = new Dictionary<KeyValuePair<string, string>, PluginBase>();

        public string ID { get; protected set; }
        public List<string> Names { get; protected set; }

        public bool AuthenticationRequired { get; protected set; }
        public bool ParameterRequired { get; protected set; }
        public bool ParameterRequiredSpecified { get; protected set; }

        public bool IsHidden { get; protected set; }
        public bool IsCommand { get; protected set; }
        public bool IsSubscribeable { get; protected set; }
        public bool IsBackgroundTask { get; protected set; }

        public string HelpText { get; protected set; }
        public Scope InstanceScope { get; protected set; }

        public PluginInfo(Type plugin)
        {
            PluginType = plugin;
            Plugin = Activator.CreateInstance(PluginType) as PluginBase;
            ID = Plugin.PluginID;

            IsCommand = Plugin is ICommand;
            IsBackgroundTask = Plugin is IBackgroundTask;

            AuthorizeAttribute authAtt = toolbox.GetAttribute<AuthorizeAttribute>(Plugin);
            AuthenticationRequired = authAtt != null;

            HelpAttribute helpAtt = toolbox.GetAttribute<HelpAttribute>(Plugin);
            if (helpAtt != null)
            {
                HelpText = helpAtt.Help;
            }

            HiddenAttribute hidenAtt = toolbox.GetAttribute<HiddenAttribute>(Plugin);
            IsHidden = hidenAtt != null;

            NameAttribute nameAtt = toolbox.GetAttribute<NameAttribute>(Plugin);
            if (nameAtt != null)
            {
                Names = nameAtt.Names.Select(x => x.ToLower()).ToList();
            }
            else
            {
                Names = new List<string>();
            }

            ParameterRequiredAttribute paramAtt = toolbox.GetAttribute<ParameterRequiredAttribute>(Plugin);
            if (paramAtt != null)
            {
                ParameterRequiredSpecified = true;
                ParameterRequired = paramAtt.Required;
            }

            ScopeAttribute scopeAtt = toolbox.GetAttribute<ScopeAttribute>(Plugin);
            if (scopeAtt != null)
            {
                InstanceScope = scopeAtt.Scope;
            }
            else
            {
                InstanceScope = Scope.Global;
            }

            SubscribeableAttribute subAtt = toolbox.GetAttribute<SubscribeableAttribute>(Plugin);
            IsSubscribeable = subAtt != null;
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
            return plugin as T;
        }

        public PluginBase GetUserScoped(string user)
        {
            PluginBase pluginBase;
            if (!UserScoped.TryGetValue(user, out pluginBase))
            {
                pluginBase = Activator.CreateInstance(PluginType) as PluginBase;
                UserScoped[user] = pluginBase;
            }
            return pluginBase;
        }

        public PluginBase GetUserChannelScoped(string user, string channel)
        {
            KeyValuePair<string, string> key = new KeyValuePair<string, string>(user, channel);
            PluginBase pluginBase;
            if (!UserChannelScoped.TryGetValue(key, out pluginBase))
            {
                pluginBase = Activator.CreateInstance(PluginType) as PluginBase;
                UserChannelScoped[key] = pluginBase;
            }
            return pluginBase;
        }

        public PluginBase GetChannelScoped(string channel)
        {
            PluginBase pluginBase;
            if (!ChannelScoped.TryGetValue(channel, out pluginBase))
            {
                pluginBase = Activator.CreateInstance(PluginType) as PluginBase;
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
            (Plugin as IBackgroundTask).Start();
        }

        public void Stop()
        {
            if (!IsBackgroundTask)
            {
                throw new NotSupportedException();
            }
            (Plugin as IBackgroundTask).Stop();
        }

        public void Run(IrcMessage theMessage)
        {
            if (!IsCommand)
            {
                throw new NotSupportedException();
            }
            (Plugin as ICommand).Run(theMessage);
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