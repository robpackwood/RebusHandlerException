using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using Rebus.Shared;

namespace Rebus.Publisher;

public class RebusPublisherBackgroundService : BackgroundService
{
    private readonly Lazy<IBus> _busLazy;
    private IBus? _bus;

    public RebusPublisherBackgroundService(Lazy<IBus> busLazy)
    {
        _busLazy = busLazy;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Factory.StartNew(() => _bus = _busLazy.Value, cancellationToken);
        Console.WriteLine("Press any key to publish a RebusMessage");
        Console.ReadKey(true);
        await _bus!.Publish(new RebusMessage());
        Console.WriteLine($"[{DateTime.Now:T}] Published RebusMessage");

        if (!cancellationToken.IsCancellationRequested)
            await ExecuteAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _bus?.Advanced.Workers.SetNumberOfWorkers(0);
        _bus?.Dispose();
        return Task.CompletedTask;
    }
}