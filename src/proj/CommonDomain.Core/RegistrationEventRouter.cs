namespace CommonDomain.Core
{
	using System;
	using System.Collections.Generic;

	public class RegistrationEventRouter<TEvent> : IRouteEvents<TEvent>
	{
		private readonly IDictionary<Type, Action<TEvent>> handlers = new Dictionary<Type, Action<TEvent>>();
		private string aggregateTypeName;

		public virtual void Register<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : TEvent
		{
			this.handlers[typeof(TEventMessage)] = @event => handler((TEventMessage)@event);
		}
		public virtual void Register(IAggregate aggregate)
		{
			this.aggregateTypeName = aggregate.GetType().Name;
		}

		public virtual void Dispatch(object eventMessage)
		{
			Action<TEvent> action;

			if (!this.handlers.TryGetValue(eventMessage.GetType(), out action))
			{
				var message = string.Format(
					"Aggregate of type {0} raised and event of type {1}, but no handler was registered.",
					this.aggregateTypeName,
					eventMessage.GetType().Name);
				throw new HandlerForDomainEventNotFoundException(message);
			}

			action((TEvent)eventMessage);
		}
	}
}