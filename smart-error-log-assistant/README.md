# Smart Error & Log Assistant

A minimalist .NET console application that reads a local application log, sends the relevant error details to an AI model, and explains the likely cause in plain English.

The project is designed as a practical introduction to AI development for backend developers. It focuses on familiar .NET concepts such as file I/O, dependency injection, configuration, asynchronous methods, and interfaces, while adding an AI-powered debugging workflow.

## What Problem Does It Solve?

Application logs are useful but often difficult to interpret quickly. A single failure can produce a large stack trace containing nested namespaces, framework internals, exception metadata, and several lines that are unrelated to the actual root cause.

The assistant turns that raw output into a concise diagnosis. For example, instead of manually tracing a `NullReferenceException`, you can ask the assistant to explain:

- What exception occurred
- What the exception means
- Which file and line most likely caused it
- What input, dependency, or assumption may have been invalid
- What code or configuration change could prevent it
- What additional information would confirm the diagnosis

## Core Workflow

```text
Local log file
	|
	v
Read and validate text
	|
	v
Build a debugging prompt
	|
	v
Send prompt to an AI chat client
	|
	v
Print a structured diagnosis
```

The application should treat the AI response as an informed analysis, not as proof of the root cause. Developers should verify the suggested line and fix against the source code, telemetry, and reproduction steps.

## Example

### Input log

```text
2026-09-21 10:14:03 ERR Request failed for customer 1842
System.NullReferenceException: Object reference not set to an instance of an object.
   at Orders.Api.Services.OrderService.CreateAsync(CreateOrderRequest request) in /src/Orders.Api/Services/OrderService.cs:line 42
   at Orders.Api.Controllers.OrdersController.Post(CreateOrderRequest request) in /src/Orders.Api/Controllers/OrdersController.cs:line 28
```

### Possible response

```text
The request failed with a NullReferenceException in OrderService.cs at line 42.
This means the code accessed a member on an object that was null. The most likely
cause is that a customer or order lookup returned no result before a property was
accessed. Check the result of the database query before using it and return a clear
not-found response when appropriate.

To confirm this, inspect the expression at line 42 and add structured logging for
the customer ID and lookup result.
```

## Planned Features

- Read a log file supplied as a command-line argument
- Validate that the file exists and contains useful content
- Send the log to an AI model through `IChatClient`
- Summarize exceptions and stack traces in plain English
- Identify the most likely failure location
- Suggest practical debugging and remediation steps
- Handle missing configuration, empty files, oversized logs, and API failures gracefully
- Keep secrets out of source code and source control

## Technology Choices

- **.NET console application** for a small, easy-to-follow entry point
- **C#** for the application code
- **Microsoft.Extensions.AI** for the provider-neutral chat abstraction
- **Azure OpenAI** or another supported chat provider for model responses
- **Microsoft.Extensions.DependencyInjection** and configuration APIs where useful

The current source files follow this separation:

```text
Program.cs             Application entry point and Azure OpenAI wiring
CommandLineOptions.cs  CLI parsing and usage text
LogFileReader.cs       File validation, reading, and size limiting
LogSanitizer.cs        Basic secret redaction before model submission
PromptBuilder.cs       System and user prompt construction
samples/error.log      Safe sample input for local testing
```

Using `IChatClient` keeps the application code independent from a specific provider. The client can be registered through dependency injection and called asynchronously with methods such as `CompleteAsync` or `GetResponseAsync`, depending on the package version used by the implementation.

## Prerequisites

Install the following before running the application:

- [.NET SDK](https://dotnet.microsoft.com/download) compatible with the project target framework
- An Azure subscription with an Azure OpenAI resource, or credentials for another supported AI provider
- A deployed chat model, such as `gpt-4o-mini`, if using Azure OpenAI

Check the installed SDK with:

```bash
dotnet --version
```

## Getting Started

From the project directory:

```bash
dotnet restore
dotnet build
```

Create a sample log file, then run the application with its path:

```bash
dotnet run -- ./samples/error.log
```

The exact command-line options may evolve as the application is implemented. A useful initial interface is:

```text
dotnet run -- <path-to-log-file>
```

The current implementation also supports:

```bash
# Inspect the sanitized prompt without making an AI request
dotnet run -- ./samples/error.log --dry-run

# Include incident context in the analysis
dotnet run -- ./samples/error.log --context "Orders API, development environment"

# Show command-line help
dotnet run -- --help
```

The included sample logs can be used to verify the application locally:

- [`samples/error.log`](samples/error.log): `NullReferenceException` in an order service
- [`samples/tcp-connection-failure.log`](samples/tcp-connection-failure.log): TCP connection refused after retry attempts
- [`samples/external-api-invalid-parameter.log`](samples/external-api-invalid-parameter.log): external CRM API rejects an invalid `countryCode`
- [`samples/unexpected-pdf-response.log`](samples/unexpected-pdf-response.log): JSON deserialization fails because the response is a PDF

## Configuration

Copy `.env.example` to `.env` and replace the placeholder values. The application loads `.env` automatically, whether it is started from the app directory or the repository root. Existing shell environment variables take precedence over values in `.env`.

```bash
cp .env.example .env
```

Do not commit API keys, connection strings, or other secrets. The local `.env` file is ignored by Git. User secrets during local development or a managed identity in Azure are also appropriate alternatives.

For an Azure OpenAI setup, the application will typically need values equivalent to:

```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT="your-chat-deployment-name"
```

The deployment name is the name of the model deployment in Azure, not necessarily the underlying model name.

Once these variables are set, run:

```bash
dotnet run -- ./samples/error.log
```

The application uses `Microsoft.Extensions.AI` with the Azure OpenAI adapter and sends a 90-second cancellable request. The `--dry-run` mode does not require Azure credentials.

## Suggested Prompt

The system instruction should establish a focused debugging role and constrain unsupported guesses:

```text
You are an expert .NET production debugger. Analyze the supplied application log.
Explain the exception in plain English, identify the most likely failure location,
separate evidence from assumptions, and suggest practical next steps. Do not invent
source code or claim certainty when the log does not provide enough information.
Return sections titled Summary, Evidence, Likely Cause, Recommended Fix, and Next Checks.
```

The user message can contain the log text and, when available, useful context such as the service name, deployment environment, request ID, or recent change. Avoid sending secrets, access tokens, passwords, or personal data to the model.

## Recommended Implementation Structure

The first version can remain small, but separating these responsibilities makes it easier to test:

1. **Input handling** reads the file path and validates the file.
2. **Log preparation** limits excessive input and removes or redacts sensitive values.
3. **Prompt construction** combines the system instruction with the log and optional context.
4. **AI analysis** calls `IChatClient` asynchronously.
5. **Output formatting** prints the diagnosis in a readable console format.
6. **Error handling** maps file, configuration, network, and model errors to clear messages and non-zero exit codes.

## Safety and Operational Notes

- Logs can contain credentials, tokens, customer data, and internal URLs. Redact sensitive values before sending them to an external model.
- Set a maximum log size and truncate or extract the relevant exception block when necessary.
- Use cancellation tokens and request timeouts so a failed provider call does not hang the console application.
- Do not execute commands, apply code changes, or treat model output as authoritative without human review.
- Be aware that sending logs to a hosted model may have data residency, retention, and compliance implications.

## Learning Goals

By completing this project, you will practice:

- Creating and running a .NET console application
- Reading files and handling command-line arguments
- Working with asynchronous C# APIs
- Registering and consuming an interface such as `IChatClient`
- Managing configuration and secrets safely
- Designing prompts that produce consistent technical explanations
- Handling external API failures and uncertain AI output
- Adding tests around deterministic log parsing and prompt construction

## Future Improvements

- Support multiple log files or a directory of logs
- Extract only exception chains and stack traces before analysis
- Add severity and service filters
- Export the diagnosis as Markdown or JSON
- Add a confidence indicator based on available evidence
- Cache repeated analyses using a hash of the sanitized log
- Add unit tests and integration tests with a fake `IChatClient`
- Add optional Azure Application Insights ingestion
- Provide a follow-up question mode for investigating a single incident interactively

## License

This project is intended for learning and experimentation. Add a license here when the repository's distribution terms have been decided.
