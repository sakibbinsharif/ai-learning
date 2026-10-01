namespace SmartErrorLogAssistant.Tests;

public sealed class LogFileReaderTests
{
    private static readonly string SamplesDirectory = Path.Combine(AppContext.BaseDirectory, "samples");

    [Fact]
    public async Task ReadAsync_reads_every_included_sample()
    {
        var samplePaths = Directory.GetFiles(SamplesDirectory, "*.log");

        Assert.Equal(4, samplePaths.Length);

        foreach (var samplePath in samplePaths)
        {
            var content = await LogFileReader.ReadAsync(samplePath, 100_000);

            Assert.False(string.IsNullOrWhiteSpace(content));
        }
    }

    [Fact]
    public async Task ReadAsync_truncates_content_over_limit()
    {
        var path = Path.Combine(Path.GetTempPath(), $"log-{Guid.NewGuid():N}.log");

        try
        {
            await File.WriteAllTextAsync(path, "1234567890");

            var content = await LogFileReader.ReadAsync(path, 5);

            Assert.Equal("12345\n\n[Log truncated after 5 characters.]", content);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadAsync_rejects_empty_files()
    {
        var path = Path.Combine(Path.GetTempPath(), $"empty-log-{Guid.NewGuid():N}.log");

        try
        {
            await File.WriteAllTextAsync(path, "  \n");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => LogFileReader.ReadAsync(path, 100));

            Assert.Contains("log file is empty", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReadAsync_rejects_missing_files()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-log-{Guid.NewGuid():N}.log");

        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() => LogFileReader.ReadAsync(path, 100));

        Assert.Equal(Path.GetFullPath(path), exception.FileName);
    }
}