using Cortex.Mediator.Notifications;
using ResQ.API.Shared.Domain.Model.Events;

namespace ResQ.API.Shared.Application.Internal.EventHandler;

public interface IEventHandler<in TEvent> : INotificationHandler<TEvent> where TEvent : IEvent
{
    
}