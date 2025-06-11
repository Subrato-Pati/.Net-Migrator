using DotnetMigratorCLI.Services;

namespace DotnetMigratorCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            string command;
            string path;

            if (args.Length == 0)
            {
                Console.Write("Enter command (analyze, migrate, suggest_services, report): ");
                command = Console.ReadLine() ?? "";
                Console.Write("Enter path: ");
                path = Console.ReadLine() ?? "";
            }
            else
            {
                command = args[0];
                path = args.Length > 1 ? args[1] : "";
            }


            switch (command.ToLower())
            {
                case "analyze":
                    Analyzer.Run(path);
                    break;
                case "migrate":
                    Migrator.Run(path);
                    break;
                case "suggest_services":
                    MicroserviceSuggester.Run(path);
                    break;
                case "report":
                    ReportGenerator.Run(path);
                    break;
                default:
                    Console.WriteLine("Unknown command.");
                    break;
            }

        }
    }
}
