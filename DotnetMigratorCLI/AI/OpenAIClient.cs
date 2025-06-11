using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetMigratorCLI.AI
{
    internal class OpenAIClient
    {

        public static string AnalyzeCode(string code)
        {
            // TODO: Call Azure OpenAI or OpenAI API
            return "AI Suggestion: Replace HttpContext.Current with IHttpContextAccessor.";
        }

    }
}
