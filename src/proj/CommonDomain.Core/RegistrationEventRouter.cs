namespace CommonDomain.Core
{
    using System;
    using System.Collections.Generic;

    public class RegistrationEventRouter<TEvent> : IRouteEvents<TEvent>
    {
        private readonly IDictionary<Type, Action<TEvent>> handlers = new Dictionary<Type, Action<TEvent>>();
        private string _aggregateTypeName;

        public virtual void Register<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : TEvent
        {
            this.handlers[typeof(TEventMessage)] = @event => handler((TEventMessage)@event);
        }
        public virtual void Register(IAggregate aggregate)
        {
            _aggregateTypeName = aggregate.GetType().Name;
        }

        public virtual void Dispatch(object eventMessage)
        {
            Action<TEvent> action;

            if (!this.handlers.TryGetValue(eventMessage.GetType(), out action))
            {
                throw new HandlerForDomainEventNotFoundException(string.Format("Aggregate of type {0} raised and event of type {1}, but no handler was registered.", _aggregateTypeName, eventMessage.GetType().Name));
            }

            action((TEvent)eventMessage);
        }
    }
}