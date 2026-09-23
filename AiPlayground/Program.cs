using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using DotNetEnv;


// string endpoint = "https://sakib3912-9114-resource.services.ai.azure.com/openai/v1";
// string apiKey = "6pT49RMSyEQmno5SjFK3QxIdma8rtXNYss8TwWiLpxPW4pbVOvgiJQQJ99CIACMsfrFXJ3w3AAAAACOG9Wtr";
// string deploymentName = "gpt-5-mini";
 Env.Load();
string endpoint = GetRequiredEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
string apiKey = GetRequiredEnvironmentVariable("AZURE_OPENAI_API_KEY");
string deploymentName = GetRequiredEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");


Console.WriteLine($"This is a simple LLM Call. Time of the call: {DateTime.Now}");
ChatClient chatClient = new(
    deploymentName,
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        Endpoint = new Uri($"{endpoint}")
    });
ChatCompletion response = chatClient.CompleteChat("Hello there. tell me about yourself");

Console.WriteLine(response.Content[0].Text);


static string GetRequiredEnvironmentVariable(string name)
{
   
    var value = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Missing required environment variable: {name}");
    }

    return value;
}
