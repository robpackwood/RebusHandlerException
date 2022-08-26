using Rebus.Handlers;
using Rebus.Shared;

namespace Rebus.Handler;

public class RebusMessageHandler : IHandleMessages<RebusMessage>
{
    public Task Handle(RebusMessage rebusMessage)
    {
        Console.WriteLine($"[{DateTime.Now:T}] Rebus Message Handled");
        return Task.CompletedTask;
    }
}