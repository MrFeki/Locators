using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Locators.Core.Logging;

public static class LogFactory
{
    public static ILoggerFactory Create(string minimumLevel, string logFile)
    {
        if (!Enum.TryParse(minimumLevel, true, out LogEventLevel level)) level = LogEventLevel.Information;

        var logger = new LoggerConfiguration()
            .MinimumLevel.Is(level)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(logFile, rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return LoggerFactory.Create(builder => builder.AddSerilog(logger, dispose: true));
    }
}
