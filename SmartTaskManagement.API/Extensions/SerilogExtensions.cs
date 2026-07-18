using Serilog;

namespace SmartTaskManagement.API.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder host) =>
        host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services));
}
