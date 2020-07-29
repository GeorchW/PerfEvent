using System;
using System.Diagnostics;
using NUnit.Framework;
using PerfEvent;

namespace test
{
    public class Tests
    {
        [Test]
        public void TestInstructionCounting()
        {
            int externalValue = 0;
            PerformanceMetrics Benchmark(int iterations)
            {
                using var counter = new PerfCounter();
                counter.Start();
                for (int i = 0; i < iterations; i++)
                {
                    externalValue += (int)Math.Sqrt(2);
                }
                counter.Stop();
                return counter.GetResults();
            }
            for (int i = 0; i < 3; i++)
            {
                var measurement = Benchmark(1000);
            }
            int baseIterations = 1;
            var overhead = Benchmark(baseIterations);
            Console.WriteLine($"Overhead:   {overhead.Instructions}");
            int iterations = 1024;
            var finalMeasurement = Benchmark(iterations + baseIterations);
            Console.WriteLine($"Final:      {finalMeasurement.Instructions}");
            var cleaned = finalMeasurement - overhead;
            Console.WriteLine("Per iteration:");
            Console.WriteLine($"Instructions: {cleaned.Instructions / (double)iterations:0.00}");
            Console.WriteLine($"Cycles:       {cleaned.Cycles / (double)iterations:0.00}");
            Console.WriteLine($"Task clock:   {cleaned.TaskClock / (double)iterations:0.00}");

            Assert.That(overhead.Instructions, Is.GreaterThan(0));
            Assert.That(finalMeasurement.Instructions, Is.GreaterThan(overhead.Instructions));
            Assert.That(cleaned.Instructions, Is.GreaterThan(iterations / 32));

            // uncomment the following line to see the results
            Assert.Fail();
        }
    }
}
