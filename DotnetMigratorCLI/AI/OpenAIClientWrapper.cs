using Azure;
using Azure.AI.OpenAI;
using System;
using System.Threading.Tasks;

namespace DotnetMigratorCLI.AI
{
    public static class OpenAIClientWrapper
    {
        // Replace with your Azure OpenAI endpoint and key
        private static readonly string endpoint = "https://<your-resource-name>.openai.azure.com/";
        private static readonly string apiKey = "<your-api-key>";
        private static readonly string deploymentName = "<your-deployment-name>"; // e.g., "gpt-4"

        private static readonly OpenAIClient client = new OpenAIClient(
            new Uri(endpoint),
            new AzureKeyCredential(apiKey)
        );

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
