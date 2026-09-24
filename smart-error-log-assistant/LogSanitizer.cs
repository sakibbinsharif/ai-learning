using System.Text.RegularExpressions;

namespace SmartErrorLogAssistant;

/// <summary>
/// Removes sensitive values from raw log text before sending it to the model.
/// </summary>
internal static partial class LogSanitizer
{
    /// <summary>
    /// Replaces common secret-style values such as bearer tokens, connection-string passwords, and API keys with redacted placeholders.
    /// </summary>
    /// <param name="log">The raw log content to sanitize.</param>
    /// <returns>A redacted version of the input log.</returns>
    internal static string Redact(string log)
    {
        var sanitized = BearerTokenRegex().Replace(log, "Bearer [REDACTED]");
        sanitized = ConnectionStringPasswordRegex().Replace(sanitized, "$1[REDACTED]");
        sanitized = ApiKeyRegex().Replace(sanitized, "$1[REDACTED]");
        return sanitized;
    }

    /// <summary>
    /// Matches bearer tokens used in Authorization headers.
    /// </summary>
    [GeneratedRegex(@"(?i)(Bearer\s+)[A-Za-z0-9._~+/=-]+")]
    private static partial Regex BearerTokenRegex();

    /// <summary>
    /// Matches password values embedded in connection strings.
    /// </summary>
    [GeneratedRegex(@"(?i)(password\s*=\s*)[^;\s]+")]
    private static partial Regex ConnectionStringPasswordRegex();

    /// <summary>
    /// Matches common API-key and access-token assignments in log text.
    /// </summary>
    [GeneratedRegex(@"(?i)((?:api[-_]?key|access[-_]?token)\s*[:=]\s*)[^,;\s]+")]
    private static partial Regex ApiKeyRegex();
}