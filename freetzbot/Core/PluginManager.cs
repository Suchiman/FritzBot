using FritzBot.DataModel;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FritzBot.Core
{
    public class PluginManager
    {
        private static PluginManager instance;
        private List<PluginBase> Plugins = new List<PluginBase>();

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
            GetInstance().Get<IBackgroundTask>().TryLogEach(x => x.Stop());
        }

        public PluginManager()
        {
            using (DBProvider db = new DBProvider())
            {
                SimpleStorage storage = db.GetSimpleStorage("PluginManager");
                string[] ReferencedAssemblies = storage.Get<string[]>("ReferencedAssemblies", null);
                if (ReferencedAssemblies == null)
                {
                    ReferencedAssemblies = new string[]
                    {
                        "mscorlib.dll",
                        "System.dll",
                        "System.Web.dll",
                        "System.Xml.dll",
                        "System.Xml.Linq.dll",
                        "Db4objects.Db4o.dll",
                        "Db4objects.Db4o.Linq.dll",
                        "HtmlAgilityPack.dll",
                        "Mono.Reflection.dll",
                        "Newtonsoft.Json.dll"
                    };
                    storage.Store("ReferencedAssemblies", ReferencedAssemblies);
                    db.SaveOrUpdate(storage);
                }
            }
        }

        /// <summary>
        /// Instanziert die Typen, entfernt bereits vorhandene Typen mit gleichem FullName und fügt die neuen hinzu.
        /// </summary>
        public int AddDistinct(bool AutostartTask, params Type[] Types)
        {
            IEnumerable<Type> FilteredTypes = Types.Where(x => !x.IsAbstract && !x.IsInterface && !typeof(Attribute).IsAssignableFrom(x)).Where(x => typeof(PluginBase).IsAssignableFrom(x));
            Plugins.OfType<IBackgroundTask>().Where(x => FilteredTypes.Select(y => y.FullName).Contains(x.GetType().FullName)).TryLogEach(x => x.Stop());
            Plugins.RemoveAll(x => FilteredTypes.Select(y => y.FullName).Contains(x.GetType().FullName));
            List<PluginBase> NewPlugins = FilteredTypes.Select(x => Activator.CreateInstance(x)).Cast<PluginBase>().ToList<PluginBase>();
            if (AutostartTask)
            {
                NewPlugins.OfType<IBackgroundTask>().TryLogEach(x => x.Start());
            }
            Plugins.AddRange(NewPlugins);
            return NewPlugins.Count;
        }

        /// <summary>
        /// Entfernt alle ICommands und IBackgroundTasks auf die die Bedingung zutrifft.
        /// </summary>
        public int Remove(Func<PluginBase, bool> bedingung)
        {
            List<PluginBase> toremove = Plugins.Where(bedingung).ToList();
            toremove.OfType<IBackgroundTask>().TryLogEach(x => x.Stop());
            Plugins.RemoveAll(x => toremove.Contains(x));
            return toremove.Count;
        }

        /// <summary>
        /// Gibt alle Instanzen von ICommand oder IBackgroundTask zurück
        /// </summary>
        public IEnumerable<T> Get<T>() where T : class
        {
            return Plugins.OfType<T>();
        }

        /// <summary>
        /// Gibt das erste ICommand oder IBackgroundTask zurück auf das die Bedingung zutrifft
        /// </summary>
        public T Get<T>(Func<T, bool> bedingung) where T : class
        {
            return Plugins.OfType<T>().FirstOrDefault(bedingung);
        }

        /// <summary>
        /// Gibt das erste ICommand oder IBackgroundTask zurück mit dem angegebenen Namen
        /// </summary>
        public T Get<T>(string name) where T : class
        {
            return Plugins.OfType<T>().Where(x => Module.NameAttribute.IsNamed(x, name)).FirstOrDefault();
        }

        /// <summary>
        /// Initialisiert das Plugin System Asynchron
        /// </summary>
        public void BeginInit(bool AutostartTask)
        {
            new Thread(delegate() { Init(AutostartTask); }).Start();
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
            return AddDistinct(true, assembly.GetTypes().Where(x => Module.NameAttribute.IsNamed(x, name)).ToArray());
        }

        /// <summary>
        /// Kompiliert Quellcode im Arbeitsspeicher zu einem Assembly
        /// </summary>
        /// <param name="fileName">Ein Array das die Dateinamen enthält</param>
        /// <returns>Das aus den Quellcode erstellte Assembly</returns>
        public Assembly LoadSource(params string[] fileName)
        {
            CSharpCodeProvider compiler = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } }); //Default ist sonst .NET 2.0
            CompilerParameters compilerParams = new CompilerParameters();
            compilerParams.CompilerOptions = "/target:library /optimize";
            compilerParams.GenerateExecutable = false;
            compilerParams.GenerateInMemory = true;
            compilerParams.IncludeDebugInformation = false;
            compilerParams.WarningLevel = 0;
            using (DBProvider db = new DBProvider())
            {
                SimpleStorage storage = db.GetSimpleStorage("PluginManager");
                if (storage != null)
                {
                    compilerParams.ReferencedAssemblies.AddRange(storage.Get<string[]>("ReferencedAssemblies"));
                }
            }
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
    }
}