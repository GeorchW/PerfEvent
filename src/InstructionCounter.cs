using System;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace PerfEvent
{
    unsafe static class PInvoke
    {
        const string DLL_NAME = "perf-interface.so";
        [DllImport(DLL_NAME, EntryPoint = "pinvoke_start_perf")]
        public static extern IntPtr StartPerf();

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_stop_perf")]
        public static extern void StopPerf(IntPtr handles);

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_num_counters")]
        public static extern int NumCounters();

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_read_counter")]
        public static extern long ReadCounter(IntPtr handles, int offset);

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_get_counter_name")]
        public static extern byte* GetCounterName(int offset);

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_close")]
        public static extern void Close(IntPtr handles);
    }

    public class InstructionCounter : IDisposable
    {
        public long Instructions { get; private set; }
        public long Cycles { get; private set; }
        public long Branches { get; private set; }
        public long BranchMisses { get; private set; }

        public long CpuClock { get; private set; }
        public long TaskClock { get; private set; }
        public long PageFaults { get; private set; }
        public long ContextSwitches { get; private set; }
        public long CpuMigrations { get; private set; }

        IntPtr handles;

        public unsafe InstructionCounter()
        {
            handles = PInvoke.StartPerf();
            if (handles == IntPtr.Zero)
            {
                throw new System.Exception("Could not start the counter");
            }
        }

        public void Dispose()
        {
            if (handles == IntPtr.Zero) return;
            PInvoke.StopPerf(handles);
            int numCounters = PInvoke.NumCounters();
            for (int i = 0; i < numCounters; i++)
            {
                string name;
                unsafe
                {
                    var ptr = PInvoke.GetCounterName(i);
                    int len = 0;
                    while (ptr[len] != 0) len++;
                    name = Encoding.UTF8.GetString(ptr, len);
                }
                long value = PInvoke.ReadCounter(handles, i);
                this.GetType().GetProperty(name).SetValue(this, value);
                // Console.WriteLine($"{name}: {value}");
            }
            PInvoke.Close(handles);
            handles = IntPtr.Zero;
        }
    }
}
