using FritzBot.DataModel;
using FritzBot.Module;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FritzBot.Core
{
    public class PluginManager : IEnumerable<PluginInfo>
    {
        private static PluginManager instance;
        private List<PluginInfo> Plugins = new List<PluginInfo>();
        private Dictionary<string, PluginInfo> LookupDictionary = new Dictionary<string, PluginInfo>(StringComparer.OrdinalIgnoreCase);

        public static PluginManager GetInstance()
        {
            if (instance == null)
            {
                instance = new PluginManager();
            }
            return instance;
        }

        /// <summary>
        /// Fährt den PluginManager herunter
        /// </summary>
        public static void Shutdown()
        {
            GetInstance().Where(x => x.IsBackgroundTask).TryLogEach(x => x.Stop());
        }

        /// <summary>
        /// Instanziert die Typen, entfernt bereits vorhandene Typen mit gleichem FullName und fügt die neuen hinzu.
        /// </summary>
        public int AddDistinct(bool AutostartTask, params Type[] Types)
        {
            IEnumerable<Type> FilteredTypes = Types.Where(x => !x.IsAbstract && !x.IsInterface && typeof(PluginBase).IsAssignableFrom(x));
            Remove(x => FilteredTypes.Select(y => y.FullName).Contains(x.GetType().FullName));

            List<PluginInfo> NewPlugins = FilteredTypes.Select(x => new PluginInfo(x)).ToList();
            if (AutostartTask)
            {
                NewPlugins.Where(x => x.IsBackgroundTask).TryLogEach(x => x.Start());
            }
            Plugins.AddRange(NewPlugins);

            foreach (PluginInfo plugin in NewPlugins)
            {
                foreach (string name in plugin.Names)
                {
                    LookupDictionary[name] = plugin;
                }
            }

            return NewPlugins.Count;
        }

        /// <summary>
        /// Entfernt alle ICommands und IBackgroundTasks auf die die Bedingung zutrifft.
        /// </summary>
        public int Remove(Func<PluginInfo, bool> bedingung)
        {
            List<PluginInfo> toremove = Plugins.Where(bedingung).ToList();
            toremove.Where(x => x.IsBackgroundTask).TryLogEach(x => x.Stop());
            Plugins.RemoveAll(x => toremove.Contains(x));

            foreach (string Name in toremove.SelectMany(x => x.Names))
            {
                LookupDictionary.Remove(Name);
            }

            return toremove.Count;
        }

        /// <summary>
        /// Gibt das erste PluginInfo zurück mit dem angegebenen Namen
        /// </summary>
        public PluginInfo Get(string name)
        {
            PluginInfo plugin;
            if (LookupDictionary.TryGetValue(name, out plugin))
            {
                return plugin;
            }
            return null;
        }

        /// <summary>
        /// Gibt die PluginInfos zurück mit dem angegebenen Plugin Typ
        /// </summary>
        public IEnumerable<PluginInfo> GetOfType<T>()
        {
            return Plugins.Where(x => x.Plugin is T);
        }

        /// <summary>
        /// Initialisiert das Plugin System Asynchron
        /// </summary>
        public void BeginInit(bool AutostartTask)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(x => { Init(AutostartTask); }));
        }

        /// <summary>
        /// Initialisiert das Plugin System und instanziert alle Plugins
        /// </summary>
        public void Init(bool AutostartTask)
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

        public void RecycleScoped(PluginBase plugin)
        {
            PluginInfo info = Plugins.FirstOrDefault(x => x.ID == plugin.PluginID);
            info.Recycle(plugin);
        }

        /// <summary>
        /// Lädt ein oder mehrere Plugins aus den gegebenen Dateien und initialisiert sie
        /// </summary>
        /// <param name="Path"></param>
        public int LoadPluginFromFile(params string[] Path)
        {
            Assembly assembly = LoadSource(Path);
            return AddDistinct(true, assembly.GetTypes());
        }

        /// <summary>
        /// Lädt ein Plugin mit angegebenen Namen aus der gegebenen Assembly
        /// </summary>
        /// <param name="assembly">Die Assembly die den Typ beinhaltet</param>
        /// <param name="name">Der Name des Types</param>
        public int LoadPluginByName(Assembly assembly, string name)
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
        public Assembly LoadSource(params string[] fileName)
        {
            CSharpCodeProvider compiler = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } }); //Default ist sonst .NET 2.0
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.WarningLevel = 0;

            //Darf für Mono nicht den selben Namen wie die FritzBot.exe Assembly haben, liefert als CompiledAssembly sonst die FritzBot.exe Assembly
            compilerParams.OutputAssembly = "FritzBotDynamic";

            string assemblies = ConfigHelper.GetString("ReferencedAssemblies", "mscorlib.dll,System.dll,System.Core.dll,System.Web.dll,System.Xml.dll,System.Xml.Linq.dll,Db4objects.Db4o.dll,Db4objects.Db4o.Linq.dll,HtmlAgilityPack.dll,Mono.Reflection.dll,Newtonsoft.Json.dll,SmartIrc4net.dll");
            compilerParams.ReferencedAssemblies.AddRange(assemblies.Split(','));
            compilerParams.ReferencedAssemblies.Add(Path.GetFileName(Assembly.GetExecutingAssembly().Location));

            CompilerResults results = null;
            try
            {
                results = compiler.CompileAssemblyFromFile(compilerParams, fileName);
            }
            catch (Exception ex)
            {
                toolbox.Logging(ex.Message);
            }
            if (results.Errors.HasErrors)
            {
                foreach (CompilerError theError in results.Errors)
                {
                    toolbox.Logging(theError.IsWarning ? "CompilerWarnung: " : "CompilerFehler: " + theError.ErrorText);
                }
                throw new Exception("Compilation failed");
            }
            return results.CompiledAssembly;
        }

        public IEnumerator<PluginInfo> GetEnumerator()
        {
            return Plugins.GetEnumerator();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Plugins.GetEnumerator();
        }
    }

    public class PluginInfo : IBackgroundTask, ICommand
    {
        public Type PluginType { get; protected set; }
        public PluginBase Plugin { get; protected set; }

        private Dictionary<User, PluginBase> UserScoped = new Dictionary<User, PluginBase>();
        private Dictionary<string, PluginBase> ChannelScoped = new Dictionary<string, PluginBase>();
        private Dictionary<KeyValuePair<User, string>, PluginBase> UserChannelScoped = new Dictionary<KeyValuePair<User, string>, PluginBase>();

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

        public T GetScoped<T>(string channel, User user) where T : class
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

        public PluginBase GetUserScoped(User user)
        {
            PluginBase pluginBase;
            if (!UserScoped.TryGetValue(user, out pluginBase))
            {
                pluginBase = Activator.CreateInstance(PluginType) as PluginBase;
                UserScoped[user] = pluginBase;
            }
            return pluginBase;
        }

        public PluginBase GetUserChannelScoped(User user, string channel)
        {
            KeyValuePair<User, string> key = new KeyValuePair<User, string>(user, channel);
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

        public void Run(ircMessage theMessage)
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
            foreach (KeyValuePair<User, PluginBase> instance in UserScoped.Where(x => x.Value == plugin).ToList())
            {
                UserScoped.Remove(instance.Key);
            }
            foreach (KeyValuePair<KeyValuePair<User, string>, PluginBase> instance in UserChannelScoped.Where(x => x.Value == plugin).ToList())
            {
                UserChannelScoped.Remove(instance.Key);
            }
        }
    }
}