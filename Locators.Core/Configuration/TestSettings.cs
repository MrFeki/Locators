namespace Locators.Core.Configuration;

public sealed class TestSettings
{
    public string ActiveEnvironment { get; init; } = "Qa";
    public string Browser { get; init; } = "Chrome";
    public bool Headless { get; init; }
    public int ExplicitWaitSeconds { get; init; } = 10;
    public string MinimumLogLevel { get; init; } = "Information";
    public string ArtifactsDirectory { get; init; } = "artifacts";
    public Dictionary<string, EnvironmentSettings> Environments { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public EnvironmentSettings Environment => Environments.TryGetValue(ActiveEnvironment, out var value)
        ? value
        : throw new InvalidOperationException($"Environment '{ActiveEnvironment}' is not configured.");
}

public sealed class EnvironmentSettings
{
    public string BaseUrl { get; init; } = string.Empty;
}
