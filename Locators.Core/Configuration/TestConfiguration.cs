using Microsoft.Extensions.Configuration;

namespace Locators.Core.Configuration;

// A single immutable configuration instance is shared by the test process.
public sealed class TestConfiguration
{
    private static readonly Lazy<TestConfiguration> InstanceHolder = new(() => new TestConfiguration());

    private TestConfiguration()
    {
        var root = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables(prefix: "TAF_")
            .Build();

        Settings = root.Get<TestSettings>() ?? throw new InvalidOperationException("TAF configuration is invalid.");
        Validate(Settings);
    }

    public static TestConfiguration Instance => InstanceHolder.Value;
    public TestSettings Settings { get; }

    private static void Validate(TestSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Environment.BaseUrl))
            throw new InvalidOperationException($"BaseUrl is missing for environment '{settings.ActiveEnvironment}'.");
        if (settings.ExplicitWaitSeconds <= 0)
            throw new InvalidOperationException("ExplicitWaitSeconds must be greater than zero.");
    }
}
