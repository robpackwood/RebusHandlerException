using System.Reflection;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Shared;

namespace Rebus.Publisher;

public class Program
{
    public static async Task Main()
    {
        var rabbitMqConfig = new RabbitMqConfig(
            RabbitConnectionString: "amqp://localhost",
            RabbitVirtualHost: "test",
            RabbitUserName: "admin",
            RabbitPassword: "password");

        var assembly = Assembly.GetExecutingAssembly();
        var hostBuilder = RebusHostBuilder.Create<RebusPublisherBackgroundService>(assembly, rabbitMqConfig);
        var host = hostBuilder.Build();

        try
        {
            await host.RunAsync();
        }
        finally
        {
            host.Services.GetService<WindsorContainer>()?.Dispose();
            (host.Services.GetService<IHostLifetime>() as IDisposable)?.Dispose();
        }
    }
}