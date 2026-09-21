using System.Text.RegularExpressions;

namespace SmartErrorLogAssistant;

internal static partial class LogSanitizer
{
    internal static string Redact(string log)
    {
        var sanitized = BearerTokenRegex().Replace(log, "Bearer [REDACTED]");
        sanitized = ConnectionStringPasswordRegex().Replace(sanitized, "$1[REDACTED]");
        sanitized = ApiKeyRegex().Replace(sanitized, "$1[REDACTED]");
        return sanitized;
    }

    [GeneratedRegex(@"(?i)(Bearer\s+)[A-Za-z0-9._~+/=-]+")]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"(?i)(password\s*=\s*)[^;\s]+")]
    private static partial Regex ConnectionStringPasswordRegex();

    [GeneratedRegex(@"(?i)((?:api[-_]?key|access[-_]?token)\s*[:=]\s*)[^,;\s]+")]
    private static partial Regex ApiKeyRegex();
}