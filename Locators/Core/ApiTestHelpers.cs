using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Locators.Core
{
    public static class ApiTestHelpers
    {
        public static IConfigurationRoot LoadConfiguration()
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Configuration file was not found: {configPath}");

            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
        }

        public static void ConfigureLogging(IConfiguration configuration)
        {
            var minLevelText = configuration["Logging:MinLevel"]
                ?? throw new InvalidOperationException("Logging:MinLevel is missing from appsettings.json.");

            if (!Enum.TryParse<LogEventLevel>(minLevelText, true, out var minLevel))
                throw new InvalidOperationException($"Invalid Logging:MinLevel value: {minLevelText}");

            var configuredPath = configuration["Logging:FilePath"]
                ?? throw new InvalidOperationException("Logging:FilePath is missing from appsettings.json.");

            var filePath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(AppContext.BaseDirectory, configuredPath);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minLevel)
                .WriteTo.Console()
                .WriteTo.File(filePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

    }
}
