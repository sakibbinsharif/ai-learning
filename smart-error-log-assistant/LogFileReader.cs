namespace SmartErrorLogAssistant;

internal static class LogFileReader
{
    internal static async Task<string> ReadAsync(string path, int maxCharacters)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The log path cannot be empty.", nameof(path));
        }

        var fullPath = Path.GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The specified log file does not exist.", fullPath);
        }

        var content = await File.ReadAllTextAsync(fullPath);

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("The log file is empty.", nameof(path));
        }

        return content.Length <= maxCharacters
            ? content
            : $"{content[..maxCharacters]}\n\n[Log truncated after {maxCharacters:N0} characters.]";
    }
}