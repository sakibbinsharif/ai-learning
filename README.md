# ai-learning

## Smart Error Log Assistant Tests

The `smart-error-log-assistant.Tests` project contains deterministic xUnit tests for command-line parsing, log reading, secret redaction, and prompt construction. It also copies every file from `smart-error-log-assistant/samples/` into the test output and verifies that the sample logs can be read.

Run the test project from the repository root with:

```bash
dotnet test smart-error-log-assistant.Tests/smart-error-log-assistant.Tests.csproj
```