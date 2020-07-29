using System;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace PerfEvent
{
    unsafe static class PInvoke
    {
        const string DLL_NAME = "perf-interface.so";

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_init")]
        public static extern IntPtr Init();
        [DllImport(DLL_NAME, EntryPoint = "pinvoke_start_perf")]
        public static extern void StartPerf(IntPtr handles);

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_stop_perf")]
        public static extern void StopPerf(IntPtr handles);

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_num_counters")]
        public static extern int NumCounters();

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_read_counter")]
        public static extern long ReadCounter(IntPtr handles, int offset);

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_get_counter_name")]
        public static extern IntPtr GetCounterName(int offset);

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_close")]
        public static extern void Close(IntPtr handles);
    }

    public sealed class PerformanceMetrics
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

        private PerformanceMetrics() { }

        public static PerformanceMetrics operator +(PerformanceMetrics left, PerformanceMetrics right)
            => new PerformanceMetrics{
                Instructions = left.Instructions + right.Instructions,
                Cycles = left.Cycles + right.Cycles,
                Branches = left.Branches + right.Branches,
                BranchMisses = left.BranchMisses + right.BranchMisses,
                CpuClock = left.CpuClock + right.CpuClock,
                TaskClock = left.TaskClock + right.TaskClock,
                PageFaults = left.PageFaults + right.PageFaults,
                ContextSwitches = left.ContextSwitches + right.ContextSwitches,
                CpuMigrations = left.CpuMigrations + right.CpuMigrations,
            };
        public static PerformanceMetrics operator -(PerformanceMetrics left, PerformanceMetrics right)
            => new PerformanceMetrics{
                Instructions = left.Instructions - right.Instructions,
                Cycles = left.Cycles - right.Cycles,
                Branches = left.Branches - right.Branches,
                BranchMisses = left.BranchMisses - right.BranchMisses,
                CpuClock = left.CpuClock - right.CpuClock,
                TaskClock = left.TaskClock - right.TaskClock,
                PageFaults = left.PageFaults - right.PageFaults,
                ContextSwitches = left.ContextSwitches - right.ContextSwitches,
                CpuMigrations = left.CpuMigrations - right.CpuMigrations,
            };
    }

    public class PerfCounter : IDisposable
    {
        IntPtr handles;

        public unsafe PerfCounter() => handles = PInvoke.Init();

        public void Start() => PInvoke.StartPerf(handles);

        public void Stop() => PInvoke.StopPerf(handles);

        public unsafe PerformanceMetrics GetResults()
        {
            PerformanceMetrics results = (PerformanceMetrics)Activator.CreateInstance(typeof(PerformanceMetrics), true);
            int numCounters = PInvoke.NumCounters();
            for (int i = 0; i < numCounters; i++)
            {
                string name = Marshal.PtrToStringAnsi(PInvoke.GetCounterName(i));
                long value = PInvoke.ReadCounter(handles, i);
                typeof(PerformanceMetrics).GetProperty(name).SetValue(results, value);
                // Console.WriteLine($"{name}: {value}");
            }
            return results;
        }

        public void Dispose()
        {
            if (handles == IntPtr.Zero) return;
            PInvoke.StopPerf(handles);
            PInvoke.Close(handles);
            handles = IntPtr.Zero;
        }
    }
}
