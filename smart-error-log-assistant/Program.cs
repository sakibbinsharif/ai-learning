using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using SmartErrorLogAssistant;

const int MaxLogCharacters = 100_000;

Console.WriteLine("Smart Error Log Assistant starting...");
try
{
	EnvironmentFile.Load();
	var options = CommandLineOptions.Parse(args);

	if (options.ShowHelp)
	{
		Console.WriteLine(CommandLineOptions.Usage);
		return 0;
	}

	if (options.LogPath is null)
	{
		Console.Error.WriteLine("A log file path is required.");
		Console.Error.WriteLine(CommandLineOptions.Usage);
		return 2;
	}

	var rawLog = await LogFileReader.ReadAsync(options.LogPath, MaxLogCharacters);
	var sanitizedLog = LogSanitizer.Redact(rawLog);
	var prompt = PromptBuilder.BuildUserPrompt(sanitizedLog, options.Context);

	if (options.DryRun)
	{
		Console.WriteLine("Dry run: the following sanitized prompt would be sent to the AI model:\n");
		Console.WriteLine(prompt);
		return 0;
	}

	var endpoint = GetRequiredEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
	var apiKey = GetRequiredEnvironmentVariable("AZURE_OPENAI_API_KEY");
	var deployment = GetRequiredEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

	var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
	using var chatClient = azureClient.GetChatClient(deployment).AsIChatClient();

	using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(90));
	var messages = new[]
	{
		new ChatMessage(ChatRole.System, PromptBuilder.SystemPrompt),
		new ChatMessage(ChatRole.User, prompt)
	};

	var response = await chatClient.GetResponseAsync(
		messages,
		new ChatOptions { Temperature = 0.2f },
		cancellationSource.Token);

	Console.WriteLine(response.Text);
	return 0;
}
catch (FileNotFoundException exception)
{
	Console.Error.WriteLine($"Log file not found: {exception.FileName}");
	return 2;
}
catch (ArgumentException exception)
{
	Console.Error.WriteLine($"Invalid input: {exception.Message}");
	return 2;
}
catch (OperationCanceledException)
{
	Console.Error.WriteLine("The AI request timed out or was cancelled.");
	return 3;
}
catch (Azure.RequestFailedException exception)
{
	Console.Error.WriteLine($"Azure OpenAI request failed ({exception.Status}): {exception.Message}");
	return 3;
}
catch (InvalidOperationException exception)
{
	Console.Error.WriteLine(exception.Message);
	return 2;
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
