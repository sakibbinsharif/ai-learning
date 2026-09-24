namespace SmartErrorLogAssistant;

/// <summary>
/// Builds the system and user prompts sent to the model for incident analysis.
/// </summary>
internal static class PromptBuilder
{
    /// <summary>
    /// Defines the model instructions that guide the investigation and response style.
    /// </summary>
    internal const string SystemPrompt = """
        You are an expert .NET production debugger. Analyze the supplied application log.
        Explain the exception in plain English, identify the most likely failure location,
        separate evidence from assumptions, and suggest practical next steps. Do not invent
        source code or claim certainty when the log does not provide enough information.
        Return sections titled Summary, Evidence, Likely Cause, Recommended Fix, and Next Checks.
        Never recommend exposing secrets or sending sensitive log data to another service.
        """;

    /// <summary>
    /// Creates the final user prompt that includes the sanitized log and any optional incident context.
    /// </summary>
    /// <param name="sanitizedLog">The redacted log content to analyze.</param>
    /// <param name="context">Optional context that helps explain the environment or incident.</param>
    /// <returns>A complete prompt object formatted for the model.</returns>
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