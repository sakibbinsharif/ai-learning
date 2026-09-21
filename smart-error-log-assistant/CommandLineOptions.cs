namespace SmartErrorLogAssistant;

internal sealed record CommandLineOptions(
    string? LogPath,
    string? Context,
    bool DryRun,
    bool ShowHelp)
{
    internal const string Usage = """
        Usage:
          dotnet run -- <path-to-log-file> [options]

        Options:
          --context <text>  Add incident context, such as service or environment.
          --dry-run         Print the sanitized prompt without calling Azure OpenAI.
          --help            Show this help text.

        Required environment variables for a live analysis:
          AZURE_OPENAI_ENDPOINT
          AZURE_OPENAI_API_KEY
          AZURE_OPENAI_DEPLOYMENT
        """;

    internal static CommandLineOptions Parse(string[] args)
    {
        string? logPath = null;
        string? context = null;
        var dryRun = false;
        var showHelp = false;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--help":
                case "-h":
                    showHelp = true;
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                case "--context":
                    if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
                    {
                        throw new ArgumentException("--context requires a value.");
                    }

                    context = args[index];
                    break;
                default:
                    if (args[index].StartsWith("-", StringComparison.Ordinal))
                    {
                        throw new ArgumentException($"Unknown option: {args[index]}");
                    }

                    if (logPath is not null)
                    {
                        throw new ArgumentException("Only one log file path may be supplied.");
                    }

                    logPath = args[index];
                    break;
            }
        }

        return new CommandLineOptions(logPath, context, dryRun, showHelp);
    }
}