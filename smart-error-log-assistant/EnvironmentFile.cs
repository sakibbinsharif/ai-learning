namespace SmartErrorLogAssistant;

/// <summary>
/// Loads environment variables from a local .env file when present so configuration can be kept out of source control.
/// </summary>
internal static class EnvironmentFile
{
    /// <summary>
    /// Reads values from the nearest .env file and sets them into the process environment if they are not already defined.
    /// </summary>
    internal static void Load()
    {
        var path = FindFile(".env");

        if (path is null)
        {
            return;
        }

        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.Ordinal))
            {
                line = line[7..].TrimStart();
            }

            var separator = line.IndexOf('=');

            if (separator <= 0)
            {
                continue;
            }

            var name = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();

            if ((value.StartsWith('"') && value.EndsWith('"')) ||
                (value.StartsWith('\'') && value.EndsWith('\'')))
            {
                value = value[1..^1];
            }

            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }

    /// <summary>
    /// Searches upward from the current directory and the app base directory for a file with the specified name.
    /// </summary>
    /// <param name="fileName">The file name to locate, such as .env.</param>
    /// <returns>The full path of the nearest matching file, or null if it is not found.</returns>
    private static string? FindFile(string fileName)
    {
        foreach (var startPath in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(startPath);

            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, fileName);

                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }
}