using FritzBot.DataModel;
using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;

namespace FritzBot.Plugins
{
    [Name("sys", "mem", "ram")]
    [Help("Ein wenig Systeminfos")]
    [ParameterRequired(false)]
    class mem : PluginBase, ICommand
    {
        public void Run(IrcMessage theMessage)
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            theMessage.Answer($"Betriebssystem: {RuntimeInformation.OSDescription}, RuntimeVersion: {RuntimeInformation.FrameworkDescription} {IntPtr.Size * 8}bit, CPU's: {Environment.ProcessorCount}, RAM Verbrauch (Programm / +Runtime): {ToMB(GC.GetTotalMemory(true))}MB / {ToMB(Process.GetCurrentProcess().WorkingSet64)}MB");
        }

        private string ToMB(long value)
        {
            return Math.Round((((float)value) / 1024f / 1024f), 2).ToString();
        }
    }
}