using System.Reflection;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Shared;

namespace Rebus.HandlerWithException;

public class Program
{
    public static async Task Main()
    {
        var rabbitMqConfig = new RabbitMqConfig(
            RabbitConnectionString: "amqp://localhost",
            RabbitVirtualHost: "test",
            RabbitUserName: "admin",
            RabbitPassword: "password",
            RabbitInputQueueName: "RebusHandlerException");

        var assembly = Assembly.GetExecutingAssembly();
        var hostBuilder = RebusHostBuilder.Create<RebusBackgroundService>(assembly, rabbitMqConfig);
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