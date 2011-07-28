namespace CommonDomain.Core
{
	using System;
	using System.Collections.Generic;

	public class RegistrationEventRouter<TEvent> : IRouteEvents<TEvent>
	{
		private readonly IDictionary<Type, Action<TEvent>> handlers = new Dictionary<Type, Action<TEvent>>();
		private IAggregate regsitered;

		public virtual void Register<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : TEvent
		{
			this.handlers[typeof(TEventMessage)] = @event => handler((TEventMessage)@event);
		}
		public virtual void Register(IAggregate aggregate)
		{
			if (aggregate == null)
				throw new ArgumentNullException("aggregate");

			this.regsitered = aggregate;
		}

		public virtual void Dispatch(object eventMessage)
		{
			Action<TEvent> handler;

			if (!this.handlers.TryGetValue(eventMessage.GetType(), out handler))
				this.regsitered.ThrowHandlerNotFound(eventMessage);

			handler((TEvent)eventMessage);
		}
	}
}