using FritzBot.DataModel;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;

namespace FritzBot.Plugins
{
    [Name("sys", "mem", "ram")]
    [Help("Ein wenig Systeminfos")]
    [ParameterRequired(false)]
    class mem : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
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
                    Log.Error(ex, "Abrufen der Kernelversion fehlgeschlagen");
                }
            }
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            theMessage.Answer(String.Format("Betriebssystem: {0}, RuntimeVersion: {1} {2}bit, CPU's: {3}, RAM Verbrauch (Programm / +Runtime): {4}MB / {5}MB", os, version.Trim(), IntPtr.Size * 8, Environment.ProcessorCount, ToMB(GC.GetTotalMemory(true)), ToMB(Process.GetCurrentProcess().WorkingSet64)));
        }

        private string ToMB(long value)
        {
            return Math.Round((((float)value) / 1024f / 1024f), 2).ToString();
        }
    }
}