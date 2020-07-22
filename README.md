# PerfEvent

This is a small helper library to measure instruction counts as reported by hardware events for benchmarks. It uses the Linux `perf_event_open` syscall, which is not available on any other platform. I.e. this only works under Linux.

However, since it doesn't throw exceptions when it's not used, you should be able to include this project as a dependency under any platform, as long as you only use it under Linux.

## How do you use it?

Currently, it works as follows:

```csharp
using var instructionCounter = new InstructionCounter();
// ... do something here ...
instructionCounter.Dispose();

Console.WriteLine($"Used {instructionCounter.RecordedInstructions} instructions.");
```

The usual benchmarking rules still apply: You should generally try to call the method you're trying to benchmark before you measure a couple of times to make sure that you're not measuring the JIT, and you should measure the overhead of measuring itself, too, to get accurate results.

## Why shouldn't I just use the `perf` command?

The point of measuring 'instructions retired' instead of 'time elapsed' is that the former is much more stable than the latter. If you're wondering "did my code just get faster by this change?", but you expect only a few percent increase, it might make more sense to use instruction count as a metric instead of time, since that will get you a more reliable result, meaning that you only have to measure once or twice (as opposed to hundreds of times). 

With `perf`, you have two options to get those results:

1. Attach to a running process. This can be done just before the call that you want to measure, but you'll have to wait a little until you can be sure that `perf` attached properly. Also, you need root permissions to do so.
2. Run the entire process with `perf stat`, e.g. `perf stat dotnet run`. This, however, will measure all kinds of things that you didn't actually want to incorporate and that might produce noise, in particular JIT overhead. Also, it makes it much harder to measure individual function calls.

So, yeah, it's more attractive to use the system call directly.

## Building and running

Running the tests:

```bash
dotnet test
```

To create the NuGet package, run:

```bash
dotnet build --configuration Release
nuget pack
```

[(Obviously, you need to have NuGet installed to do so.)](https://www.nuget.org/downloads)
