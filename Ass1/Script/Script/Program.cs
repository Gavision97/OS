using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

class Script
{
    static void Main(string[] args)

    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string projectDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\"));

        string relativePath = Path.GetRelativePath(projectDirectory, currentDirectory);
        Console.WriteLine("Relative path of current project: " + relativePath);
        if (args.Length < 2)
        {

            Console.WriteLine("Usage: Program.exe [NumberOfInstances] [Argument]");
            return;
        }
        int numberOfInstances = int.Parse(args[0]);
        string iterations = args[1];

        Stopwatch[] stopwatches = new Stopwatch[numberOfInstances];
        string filePath = @"C:\Users\ilay\source\repos\Script\Script\bin\Debug\times.txt";


        Parallel.For(0, numberOfInstances, i =>
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Users\ilay\source\repos\CPU-Process\CPU-Process\bin\Debug\net8.0\CPU-Process.exe",
                Arguments = iterations
            };

            stopwatches[i] = Stopwatch.StartNew();
            Process.Start(startInfo)?.WaitForExit();
            stopwatches[i].Stop();

            Console.WriteLine($"Instance {i + 1} took {stopwatches[i].ElapsedMilliseconds} milliseconds.");
        });

        double sum = 0;
        for(int i=0; i < stopwatches.Length; i++){
            sum += stopwatches[i].ElapsedMilliseconds;
        }
        sum = sum / numberOfInstances;
        using (StreamWriter writer = new StreamWriter(filePath, append:true))
        {
            writer.WriteLine($"AvgTime:{sum}Milisecs NumberOfProcess:{numberOfInstances} NumberOfIterations: {iterations}");

        }

        Console.WriteLine($"Started {numberOfInstances} instances of the process with {iterations} iterations.");
    }
}
