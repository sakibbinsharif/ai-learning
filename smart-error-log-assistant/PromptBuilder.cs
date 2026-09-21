namespace SmartErrorLogAssistant;

internal static class PromptBuilder
{
    internal const string SystemPrompt = """
        You are an expert .NET production debugger. Analyze the supplied application log.
        Explain the exception in plain English, identify the most likely failure location,
        separate evidence from assumptions, and suggest practical next steps. Do not invent
        source code or claim certainty when the log does not provide enough information.
        Return sections titled Summary, Evidence, Likely Cause, Recommended Fix, and Next Checks.
        Never recommend exposing secrets or sending sensitive log data to another service.
        """;

    internal static string BuildUserPrompt(string sanitizedLog, string? context)
    {
        var contextSection = string.IsNullOrWhiteSpace(context)
            ? "No additional incident context was provided."
            : context;

        return $"""
            Analyze this sanitized application log.

            Additional context:
            {contextSection}

            Log:
            ```text
            {sanitizedLog}
            ```
            """;
    }
}