using System;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace PerfEvent
{
    static class PInvoke
    {
        const string DLL_NAME = "obj/perf-interface.so";
        [DllImport(DLL_NAME, EntryPoint = "pinvoke_start_perf")]
        public static extern unsafe int StartPerf();

        [DllImport(DLL_NAME, EntryPoint = "pinvoke_stop_perf")]
        public static extern long StopPerf(int fd);
    }

    public class InstructionCounter : IDisposable
    {
        public long RecordedInstructions { get; private set; }

        int fd;

        public unsafe InstructionCounter()
        {
            fd = PInvoke.StartPerf();
            if (fd == -1)
            {
                throw new System.Exception("Could not start the counter");
            }
        }

        public void Dispose()
        {
            RecordedInstructions = PInvoke.StopPerf(fd);
        }
    }
}
