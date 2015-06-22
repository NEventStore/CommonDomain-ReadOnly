namespace CommonDomain.Core
{
	using System;
	using System.Collections.Generic;

	public class RegistrationEventRouter : IRouteEvents
	{
		private readonly IDictionary<Type, Action<object>> handlers = new Dictionary<Type, Action<object>>();
		private IAggregate registered;

		public virtual void Register<T>(Action<T> handler)
		{
			this.handlers[typeof(T)] = @event => handler((T)@event);
		}
		public virtual void Register(IAggregate aggregate)
		{
			if (aggregate == null)
				throw new ArgumentNullException("aggregate");

			this.registered = aggregate;
		}

		public virtual void Dispatch(object eventMessage)
		{
			Action<object> handler;

			if (!this.handlers.TryGetValue(eventMessage.GetType(), out handler))
				this.registered.ThrowHandlerNotFound(eventMessage);

			handler(eventMessage);
		}
	}
}
