namespace SmartErrorLogAssistant.Tests;

public sealed class LogSanitizerTests
{
    [Fact]
    public void Redact_removes_supported_secret_formats()
    {
        const string log = "Authorization: Bearer abc.def_123\npassword=super-secret;\napi-key: key-value";

        var sanitized = LogSanitizer.Redact(log);

        Assert.DoesNotContain("abc.def_123", sanitized);
        Assert.DoesNotContain("super-secret", sanitized);
        Assert.DoesNotContain("key-value", sanitized);
        Assert.Equal(3, CountOccurrences(sanitized, "[REDACTED]"));
    }

    [Fact]
    public void Redact_preserves_non_secret_log_content()
    {
        const string log = "Request failed with status 500 for customer 1842.";

        Assert.Equal(log, LogSanitizer.Redact(log));
    }

    private static int CountOccurrences(string value, string search)
    {
        return value.Split(search, StringSplitOptions.None).Length - 1;
    }
}