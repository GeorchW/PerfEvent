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
            long CountInstructions(int iterations)
            {
                var counter = new InstructionCounter();
                using (counter)
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        externalValue += (int)Math.Sqrt(2);
                    }
                }
                return counter.RecordedInstructions;
            }
            for (int i = 0; i < 3; i++)
            {
                long count = CountInstructions(1);
                Console.WriteLine(count);
            }
            int baseIterations = 1;
            long overhead = CountInstructions(baseIterations);
            Console.WriteLine($"Overhead:   {overhead}");
            int iterations = 1024;
            long finalMeasurement = CountInstructions(iterations + baseIterations);
            Console.WriteLine($"Final:      {finalMeasurement}");
            Console.WriteLine($"Instructions per iteration (minus overhead): {(finalMeasurement - overhead) / (double)iterations:0.00}");
            
            Assert.That(overhead, Is.GreaterThan(0));
            Assert.That(finalMeasurement, Is.GreaterThan(overhead));
            Assert.That(finalMeasurement - overhead, Is.GreaterThan(iterations / 32));

            // uncomment the following line to see the results
            // Assert.Fail();
        }
    }
}
