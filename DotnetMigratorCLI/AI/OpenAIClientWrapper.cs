using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DotnetMigratorCLI.AI
{
    public static class OpenAIClientWrapper
    {
        private static readonly IConfiguration config;
        private static readonly OpenAIClient client;
        private static readonly string deploymentName;

        static OpenAIClientWrapper()
        {
            config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string endpoint = config["AzureOpenAI:Endpoint"];
            string apiKey = config["AzureOpenAI:ApiKey"];
            deploymentName = config["AzureOpenAI:DeploymentName"];

            client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        }

        public static async Task<string> GetSuggestionAsync(string prompt)
        {
            var options = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are a .NET modernization assistant."),
                    new ChatMessage(ChatRole.User, prompt)
                },
                MaxTokens = 800,
                Temperature = 0.3f
            };

            var response = await client.GetChatCompletionsAsync(deploymentName, options);
            return response.Value.Choices[0].Message.Content;
        }
    }
}
