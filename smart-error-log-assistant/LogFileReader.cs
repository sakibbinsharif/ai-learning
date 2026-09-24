namespace SmartErrorLogAssistant;

/// <summary>
/// Reads log files and limits the returned content to a configured character count.
/// </summary>
internal static class LogFileReader
{
    /// <summary>
    /// Reads a log file and returns its contents, truncated to a maximum number of characters when needed.
    /// </summary>
    /// <param name="path">The file path for the log to read.</param>
    /// <param name="maxCharacters">The maximum number of characters to return before appending a truncation notice.</param>
    /// <returns>The full or truncated log content.</returns>
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