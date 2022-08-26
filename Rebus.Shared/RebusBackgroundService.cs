using Microsoft.Extensions.Hosting;
using Rebus.Bus;

namespace Rebus.Shared;

public class RebusBackgroundService : BackgroundService
{
    private readonly Lazy<IBus> _busLazy;
    private IBus? _bus;

    public RebusBackgroundService(Lazy<IBus> busLazy)
    {
        _busLazy = busLazy;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Factory.StartNew(() => _bus = _busLazy.Value, cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.Factory.StartNew(
            () =>
            {
                _bus?.Advanced.Workers.SetNumberOfWorkers(0);
                _bus?.Dispose();
            }, cancellationToken);
    }
}