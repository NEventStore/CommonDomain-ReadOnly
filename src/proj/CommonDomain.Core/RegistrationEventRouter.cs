namespace CommonDomain.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class RegistrationEventRouter<TEvent> : IRouteEvents<TEvent>
	{
		private readonly IDictionary<Type, Action<TEvent>> handlers = new Dictionary<Type, Action<TEvent>>();
		
		public virtual void Register<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : TEvent
		{
			this.handlers[typeof(TEventMessage)] = @event => handler((TEventMessage)@event);
		}
		public virtual void Register(IAggregate aggregate)
		{
			// no-op
		}

		public virtual void Dispatch(object eventMessage)
		{
			this.handlers[eventMessage.GetType()]((TEvent)eventMessage);
		}
	}
}