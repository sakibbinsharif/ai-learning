namespace SmartErrorLogAssistant.Tests;

public sealed class PromptBuilderTests
{
    [Fact]
    public void BuildUserPrompt_includes_log_and_context()
    {
        var prompt = PromptBuilder.BuildUserPrompt("error details", "Orders API in production");

        Assert.Contains("Orders API in production", prompt);
        Assert.Contains("error details", prompt);
        Assert.Contains("```text", prompt);
    }

    [Fact]
    public void BuildUserPrompt_uses_default_context_when_context_is_blank()
    {
        var prompt = PromptBuilder.BuildUserPrompt("error details", " ");

        Assert.Contains("No additional incident context was provided.", prompt);
    }

    [Fact]
    public void SystemPrompt_requires_evidence_based_sections()
    {
        Assert.Contains("Summary", PromptBuilder.SystemPrompt);
        Assert.Contains("Evidence", PromptBuilder.SystemPrompt);
        Assert.Contains("Never recommend exposing secrets", PromptBuilder.SystemPrompt);
    }
}