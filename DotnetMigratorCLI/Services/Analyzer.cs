using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text.Json;

namespace DotnetMigratorCLI.Services
{
    public static class Analyzer
    {
        public static void Run(string path)
        {
            Console.WriteLine($"[Analyzer] Scanning project at {path}...");

            var csprojFiles = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories);
            if (csprojFiles.Length == 0)
            {
                Console.WriteLine("❌ No .csproj file found.");
                return;
            }

            string csproj = File.ReadAllText(csprojFiles[0]);

            // Detect target framework
            string framework = Regex.Match(csproj, @"<TargetFrameworkVersion>(.*?)</TargetFrameworkVersion>").Groups[1].Value;
            if (string.IsNullOrEmpty(framework))
                framework = "Unknown (possibly legacy .NET Framework)";

            // List NuGet packages
            var packages = Regex.Matches(csproj, @"<PackageReference Include=\""(.*?)\"" Version=\""(.*?)\""")
                                .Cast<Match>()
                                .Select(m => new { Name = m.Groups[1].Value, Version = m.Groups[2].Value })
                                .ToList();

            // Scan for deprecated APIs
            var deprecatedApis = new Dictionary<string, string>
            {
                { "HttpContext.Current", "Use IHttpContextAccessor" },
                { "AppDomain", "Use AssemblyLoadContext in .NET Core" },
                { "WebForms", "Consider Razor Pages or Blazor" },
                { "ConfigurationManager.AppSettings", "Use IConfiguration from Microsoft.Extensions.Configuration" }
            };

            var codeFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
            var foundApis = new List<Dictionary<string, string>>();

            foreach (var api in deprecatedApis)
            {
                foreach (var file in codeFiles)
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains(api.Key))
                    {
                        foundApis.Add(new Dictionary<string, string>
                        {
                            { "api", api.Key },
                            { "file", Path.GetFileName(file) },
                            { "suggestion", api.Value }
                        });
                    }
                }
            }

            // Prepare output
            var result = new
            {
                project_path = path,
                detected_framework = framework,
                nuget_packages = packages,
                deprecated_apis = foundApis
            };

            Directory.CreateDirectory("output");

            // Write JSON
            File.WriteAllText("output/analysis.json", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

            // Write Markdown
            var md = new List<string>
            {
                "# Migration Analysis Report",
                $"**Project Path**: {path}",
                $"**Detected Framework**: {framework}",
                "",
                "## 📦 NuGet Packages"
            };

            if (packages.Count == 0)
                md.Add("- None");
            else
                md.AddRange(packages.Select(p => $"- {p.Name} ({p.Version})"));

            md.Add("\n## ⚠️ Deprecated API Usage");
            if (foundApis.Count == 0)
                md.Add("- None found");
            else
                md.AddRange(foundApis.Select(api => $"- `{api["api"]}` in `{api["file"]}` → Suggestion: {api["suggestion"]}"));

            md.Add($"\n*Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");

            File.WriteAllText("output/analysis_report.md", string.Join("\n", md));

            Console.WriteLine("✅ Analysis exported to 'output/analysis.json' and 'output/analysis_report.md'");
        }
    }
}
