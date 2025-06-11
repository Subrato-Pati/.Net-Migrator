
using System;
using HakunaMatata.Services;

namespace HakunaMatata
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a path to analyze.");
                return;
            }

            string path = args[0];
            ProjectAnalyzer analyzer = new ProjectAnalyzer();
            analyzer.Analyze(path);
        }
    }
}
