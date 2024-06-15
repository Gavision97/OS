using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main()
    {
        const int NumMeasurements = 10;
        long[] processExecutionTimes = new long[NumMeasurements];
        long[] threadExecutionTimes = new long[NumMeasurements];

        Random random = new Random();

        for (int i = 0; i < NumMeasurements; i++)
        {
            // Measure process execution time
            Stopwatch processStopwatch = Stopwatch.StartNew();
            ExecuteProcess("Hello World " + random.Next());
            processStopwatch.Stop();
            processExecutionTimes[i] = processStopwatch.ElapsedMilliseconds;

            // Measure thread execution time
            Stopwatch threadStopwatch = Stopwatch.StartNew();
            ExecuteThread("Hello World " + random.Next());
            threadStopwatch.Stop();
            threadExecutionTimes[i] = threadStopwatch.ElapsedMilliseconds;

            // Wait a bit between measurements to avoid overlapping time measurements
            Thread.Sleep(100);
        }

        // Output the results in a table format
        Console.WriteLine("Results:");
        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine("| Run # | Process Execution Time (ms) | Thread Execution Time (ms) |");
        Console.WriteLine("-------------------------------------------------");
        for (int i = 0; i < NumMeasurements; i++)
        {
            Console.WriteLine($"| {i + 1,-5} | {processExecutionTimes[i],-28} | {threadExecutionTimes[i],-27} |");
        }
        Console.WriteLine("-------------------------------------------------");
    }

    static void ExecuteProcess(string message)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "dotnet"; // Assuming 'dotnet' command is available in PATH
        startInfo.Arguments = $"exec --output \"\" -e Console.WriteLine(\"{message}\")";
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;

        using (Process process = Process.Start(startInfo))
        {
            process.WaitForExit();
        }
    }

    static void ExecuteThread(string message)
    {
        Thread thread = new Thread(() =>
        {
            Console.WriteLine(message);
        });

        thread.Start();
        thread.Join(); // Wait for thread to finish execution
    }
}

