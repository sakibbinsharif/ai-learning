using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using DotNetEnv;
using System.IO;
using Azure;


LoadEnvironment();
string endpoint = GetRequiredEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
string apiKey = GetRequiredEnvironmentVariable("AZURE_OPENAI_API_KEY");
string deploymentName = GetRequiredEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

endpoint = endpoint.TrimEnd('/');
endpoint = endpoint.EndsWith("/openai/v1", StringComparison.OrdinalIgnoreCase)
    ? endpoint[..^"/openai/v1".Length]
    : endpoint;

Console.WriteLine($"This is a simple LLM Call. Time of the call: {DateTime.Now}");
AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
// ChatClient chatClient = azureOpenAIClient.GetChatClient(deploymentName);
// ChatCompletion response = chatClient.CompleteChat("Hello there. tell me about yourself");

var systemMessage = new SystemChatMessage("You are a helpful assistant that provides information about yourself. Client name is Sakib. he is a software developer. This is a test app to make sure everything works correctly.");

var conversation = new List<ChatMessage>();
conversation.Capacity = 10;

conversation.Add(systemMessage);



Console.WriteLine("Please enter your message to the AI. Type 'exit' to quit.");

var chatClient = azureOpenAIClient.GetChatClient(deploymentName);

while (true)
{
	Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("You: ");
    var userMessage = Console.ReadLine();

    if (userMessage.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

	conversation.Add(new UserChatMessage(userMessage));

    var result = await chatClient.CompleteChatAsync(conversation);

	Console.ForegroundColor = ConsoleColor.Red;
    foreach (var part in result.Value.Content)
    {
		var agentResponse = new AssistantChatMessage(part.Text);
        conversation.Add(agentResponse);
		Console.WriteLine($"AI: {part.Text}");
    }
}



static string GetRequiredEnvironmentVariable(string name)
{
   
    var value = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Missing required environment variable: {name}");
    }

    return value;
}
static void LoadEnvironment()
{
	// Try to load .env from the build output (AppContext.BaseDirectory) so Visual Studio debugging loads the same values
	var outputEnvPath = Path.Combine(AppContext.BaseDirectory, ".env");
	if (File.Exists(outputEnvPath))
	{
		Env.Load(outputEnvPath);
	}
	else
	{
		// Fallback: try default lookup (current directory / parent directories)
		Env.Load();
	}
}
