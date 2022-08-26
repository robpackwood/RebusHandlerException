using Rebus.Handlers;
using Rebus.Shared;

namespace Rebus.HandlerWithException;

public class RebusMessageHandlerWithException : IHandleMessages<RebusMessage>
{
    public Task Handle(RebusMessage rebusMessage)
    {
        Console.WriteLine($"[{DateTime.Now:T}] Rebus Message Handled");
        throw new NotImplementedException();
    }
}