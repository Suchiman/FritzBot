using FritzBot.DataModel;
using System;
using System.Diagnostics;
using System.Reflection;

namespace FritzBot.Plugins
{
    [Module.Name("sys", "mem", "ram")]
    [Module.Help("Ein wenig Systeminfos")]
    [Module.ParameterRequired(false)]
    class mem : PluginBase, ICommand
    {
        public void Run(ircMessage theMessage)
        {
            string version = Environment.Version.ToString(2);
            string os = Environment.OSVersion.ToString();
            Type t = Type.GetType("Mono.Runtime");
            if (t != null)
            {
                MethodInfo displayName = t.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                if (displayName != null)
                {
                    version = (string)displayName.Invoke(null, null);
                    version = "Mono " + version.Substring(0, version.IndexOf('('));
                }
                try
                {
                    ProcessStartInfo pi = new ProcessStartInfo("uname", "-s -r -m");
                    pi.RedirectStandardOutput = true;
                    pi.UseShellExecute = false;
                    Process p = new Process();
                    p.StartInfo = pi;
                    p.Start();
                    os = p.StandardOutput.ReadLine();
                }
                catch (Exception ex)
                {
                    toolbox.Logging(ex);
                }
            }
            theMessage.Answer(String.Format("Betriebssystem: {0}, RuntimeVersion: {1} {2}bit, CPU's: {3}, RAM Verbrauch (Programm / +Runtime): {4}MB / {5}MB", os, version.Trim(), IntPtr.Size * 8, Environment.ProcessorCount, ToMB(GC.GetTotalMemory(true)), ToMB(System.Diagnostics.Process.GetCurrentProcess().WorkingSet64)));
        }

        private string ToMB(long value)
        {
            return Math.Round((((float)value) / 1024f / 1024f), 2).ToString();
        }
    }
}