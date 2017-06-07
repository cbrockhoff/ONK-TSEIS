using System;
using System.Threading.Tasks;
using Shared.Contracts;

namespace Shared.Messaging
{
    public interface IMessageBus
    {
        Task Publish(IEvent @event);
        IDisposable Subscribe<TMessage>(Func<TMessage, Task> onEvent) where TMessage : IEvent;

        Task Send(ICommand command);
        IDisposable Receive<TMessage>(Func<TMessage, Task> onCommand) where TMessage : ICommand;
    }
}
