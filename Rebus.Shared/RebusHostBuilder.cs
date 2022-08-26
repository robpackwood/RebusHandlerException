using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.CastleWindsor;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Retry.Simple;

namespace Rebus.Shared;

public static class RebusHostBuilder
{
    public static IHostBuilder Create<TBackgroundService>(Assembly assembly, RabbitMqConfig rabbitMqConfig)
        where TBackgroundService : BackgroundService
    {
        var serviceProviderFactory = new WindsorServiceProviderFactory();

        return Host.CreateDefaultBuilder()
            .UseServiceProviderFactory(serviceProviderFactory)
            .ConfigureServices(services =>
                services.AddHostedService<TBackgroundService>()
                    .AddLogging(builder =>
                        builder.AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddConsole())
            )
            .ConfigureContainer<WindsorContainer>((_, container) =>
            {
                if (!rabbitMqConfig.IsSendOnlyEndpoint)
                    container.AutoRegisterHandlersFromAssembly(assembly);

                var rebusConfigurer = Configure
                    .With(new CastleWindsorContainerAdapter(container))
                    .Logging(l => l.None())
                    .Options(configurer => configurer.SimpleRetryStrategy(maxDeliveryAttempts: 1));

                rebusConfigurer.Transport(configurer =>
                {
                    var rabbitMqOptionsBuilder =
                        rabbitMqConfig.IsSendOnlyEndpoint
                            ? configurer.UseRabbitMqAsOneWayClient(rabbitMqConfig.RabbitConnectionString)
                            : configurer.UseRabbitMq(rabbitMqConfig.RabbitConnectionString, rabbitMqConfig.RabbitInputQueueName);

                    rabbitMqOptionsBuilder.CustomizeConnectionFactory(customizer =>
                    {
                        customizer.UserName = rabbitMqConfig.RabbitUserName;
                        customizer.Password = rabbitMqConfig.RabbitPassword;
                        customizer.VirtualHost = rabbitMqConfig.RabbitVirtualHost;
                        return customizer;
                    }).SetPublisherConfirms(true);
                });

                container.Register(Component.For<Lazy<IBus>>()
                    .Instance(new Lazy<IBus>(() =>
                    {
                        var bus = rebusConfigurer.Start();

                        if (!rabbitMqConfig.IsSendOnlyEndpoint)
                            ConfigureMessageHandlerSubscribers(bus, assembly);

                        Console.WriteLine($"[{DateTime.Now:T}] Bus started");
                        return bus;
                    })));
            });
    }

    public static void ConfigureMessageHandlerSubscribers(IBus bus, Assembly assembly)
    {
        var handlerType = typeof(IHandleMessages);

        var messageHandlerTypes = assembly.GetTypes().Where(
            type => handlerType.IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface);

        var uniqueMessageHandlerTypes = new HashSet<Type>(
            from messageHandlerType in messageHandlerTypes
            from messageHandlerInterface in messageHandlerType.GetInterfaces()
            where messageHandlerInterface.IsGenericType
            select messageHandlerInterface.GetGenericArguments()[0]);

        var tasks = (from uniqueMessageHandlerType in uniqueMessageHandlerTypes
            select bus.Subscribe(uniqueMessageHandlerType)).ToList();

        Task.WhenAll(tasks).GetAwaiter().GetResult();
    }
}