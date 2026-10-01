namespace SmartErrorLogAssistant.Tests;

public sealed class CommandLineOptionsTests
{
    [Fact]
    public void Parse_defaults_to_empty_options()
    {
        var options = CommandLineOptions.Parse([]);

        Assert.Null(options.LogPath);
        Assert.Null(options.Context);
        Assert.False(options.DryRun);
        Assert.False(options.ShowHelp);
    }

    [Fact]
    public void Parse_reads_path_context_dry_run_and_help()
    {
        var options = CommandLineOptions.Parse(["samples/error.log", "--context", "production", "--dry-run", "--help"]);

        Assert.Equal("samples/error.log", options.LogPath);
        Assert.Equal("production", options.Context);
        Assert.True(options.DryRun);
        Assert.True(options.ShowHelp);
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("-verbose")]
    public void Parse_rejects_unknown_options(string argument)
    {
        var exception = Assert.Throws<ArgumentException>(() => CommandLineOptions.Parse([argument]));

        Assert.Contains("Unknown option", exception.Message);
    }

    [Fact]
    public void Parse_rejects_missing_context_value()
    {
        var exception = Assert.Throws<ArgumentException>(() => CommandLineOptions.Parse(["--context"]));

        Assert.Contains("--context requires a value", exception.Message);
    }

    [Fact]
    public void Parse_rejects_multiple_paths()
    {
        var exception = Assert.Throws<ArgumentException>(() => CommandLineOptions.Parse(["first.log", "second.log"]));

        Assert.Contains("Only one log file path", exception.Message);
    }
}