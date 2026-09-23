using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using DotNetEnv;
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



var chatClient = azureOpenAIClient.GetChatClient(deploymentName);
var result = await chatClient.CompleteChatAsync(
    [
        new SystemChatMessage("You are a helpful assistant that provides information about yourself."),
        new UserChatMessage("Hello there. tell me about yourself")
    ]);

foreach (var part in result.Value.Content)
{
    Console.WriteLine(part.Text);
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
	Env.Load();
	var runtimeCounter = Environment.GetEnvironmentVariable("RuntimeCounter");
	int counter = 0;
	if (runtimeCounter == null)
	{
		counter = 1;
		Environment.SetEnvironmentVariable("RuntimeCounter", "1", EnvironmentVariableTarget.Process);
	}
	else
	{
		counter = int.Parse(runtimeCounter);
		counter++;
		Environment.SetEnvironmentVariable("RuntimeCounter", counter.ToString(), EnvironmentVariableTarget.Process);
	}
	Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}" +
			$" RuntimeCounter: {counter.ToString()}" +
			$" - AiPlayground starting...");

}