
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using System.Linq;


namespace HakunaMatata.Services
{
    public class ProjectAnalyzer
    {
        public void Analyze(string path)
        {
            if (Directory.Exists(path))
            {
                var slnFile = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories).FirstOrDefault();
                if (slnFile != null)
                {
                    Console.WriteLine($"Solution found: {slnFile}");
                    LoadSolution(slnFile);
                    return;
                }

                var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
                if (csprojFiles.Length == 1)
                {
                    Console.WriteLine($"Project found: {csprojFiles[0]}");
                    LoadProject(csprojFiles[0]);
                }
                else if (csprojFiles.Length > 1)
                {
                    Console.WriteLine("Multiple .csproj files found:");
                    for (int i = 0; i < csprojFiles.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}. {csprojFiles[i]}");
                    }
                    Console.WriteLine("Analyzing all projects...");
                    foreach (var proj in csprojFiles)
                    {
                        LoadProject(proj);
                    }
                }
                else
                {
                    Console.WriteLine("No .sln or .csproj files found in the directory.");
                }
            }
            else if (File.Exists(path))
            {
                if (path.EndsWith(".sln"))
                {
                    LoadSolution(path);
                }
                else if (path.EndsWith(".csproj"))
                {
                    LoadProject(path);
                }
                else
                {
                    Console.WriteLine("Unsupported file type. Please provide a .sln or .csproj file.");
                }
            }
            else
            {
                Console.WriteLine("Invalid path. Please provide a valid file or directory.");
            }
        }


        private async void LoadSolution(string slnPath)
        {
            MSBuildLocator.RegisterDefaults();

            using var workspace = MSBuildWorkspace.Create();
            Console.WriteLine("Loading solution...");
            var solution = await workspace.OpenSolutionAsync(slnPath);

            foreach (var project in solution.Projects)
            {
                Console.WriteLine($"\n📦 Project: {project.Name}");
                await AnalyzeProject(project);
            }
        }

        private async void LoadProject(string csprojPath)
        {
            MSBuildLocator.RegisterDefaults();

            using var workspace = MSBuildWorkspace.Create();
            Console.WriteLine("Loading project...");
            var project = await workspace.OpenProjectAsync(csprojPath);

            Console.WriteLine($"\n📦 Project: {project.Name}");
            await AnalyzeProject(project);
        }


        private async Task AnalyzeProject(Project project)
        {
            Console.WriteLine($"Target Framework: {project.ParseOptions?.Language}");

            var compilation = await project.GetCompilationAsync();
            if (compilation == null)
            {
                Console.WriteLine("❌ Failed to compile project.");
                return;
            }

            Console.WriteLine($"Referenced Assemblies: {compilation.ReferencedAssemblyNames.Count()}");
            Console.WriteLine("Analyzing syntax trees...");

            foreach (var tree in compilation.SyntaxTrees)
            {
                var root = await tree.GetRootAsync();
                var deprecatedUsages = root.DescendantNodes()
                .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.UsingDirectiveSyntax>()
                .Where(u => u.Name.ToString().StartsWith("System.Web"));

                foreach (var usage in deprecatedUsages)
                {
                    Console.WriteLine($"⚠️ Deprecated usage found: {usage.Name} in {tree.FilePath}");
                }
            }

            Console.WriteLine("✅ Analysis complete.");
        }


    }
}
