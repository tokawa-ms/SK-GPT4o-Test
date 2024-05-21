using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Configuration;

namespace SK_GPT4o_Test
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            string? endpoint = configuration["Endpoint"];
            string? deployName = configuration["DeployName"];
            string? apiKey = configuration["ApiKey"];

            if (endpoint is null || deployName is null || apiKey is null)
            {
                Console.WriteLine("Please set Azure OpenAI credentials in appsettings.json.");

                return;
            }

            // Handle the API version manually
            var client = new HttpClient(new ApiVersionHandler());

            // Create kernel
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(deployName, endpoint, apiKey,httpClient:client);
            var kernel = builder.Build();

            // Create chat history
            var history = new ChatHistory();

            // Get chat completion service
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Start the conversation
            Console.Write("User (press CTRL+Z to exit) > ");
            string? userInput;
            while (!String.IsNullOrEmpty(userInput = Console.ReadLine()))
            {
                // Add user input
                history.AddUserMessage(userInput);

                // Enable auto function calling
                OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                };

                // Get the response from the AI
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel);

                // Print the results
                Console.WriteLine("Assistant > " + result);

                // Add the message from the agent to the chat history
                history.AddMessage(result.Role, result.Content ?? string.Empty);

                // Get user input again
                Console.Write("User (press CTRL+Z to exit) > ");
            }
        }
    }
}
