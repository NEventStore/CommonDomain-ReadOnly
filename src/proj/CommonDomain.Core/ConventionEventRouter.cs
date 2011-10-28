namespace CommonDomain.Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class ConventionEventRouter : IRouteEvents
	{
		private readonly IDictionary<Type, Action<object>> handlers = new Dictionary<Type, Action<object>>();
		private IAggregate registered;

		public virtual void Register<T>(Action<T> handler)
		{
			if (handler == null)
				throw new ArgumentNullException("handler");

			this.Register(typeof(T), @event => handler((T)@event));
		}

		public virtual void Register(IAggregate aggregate)
		{
			if (aggregate == null)
				throw new ArgumentNullException("aggregate");

			this.registered = aggregate;

			// Get instance methods named Apply with one parameter returning void
			var applyMethods = aggregate.GetType()
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(m => m.Name == "Apply" && m.GetParameters().Length == 1 && m.ReturnParameter.ParameterType == typeof(void))
				.Select(m => new
				{
					Method = m,
					MessageType = m.GetParameters().Single().ParameterType
				});

			foreach (var apply in applyMethods)
			{
				var applyMethod = apply.Method;
				this.handlers.Add(apply.MessageType, m => applyMethod.Invoke(aggregate, new[] { m as object }));
			}
		}

		public virtual void Dispatch(object eventMessage)
		{
			if (eventMessage == null)
				throw new ArgumentNullException("eventMessage");

			Action<object> handler;
            if (this.handlers.TryGetValue(eventMessage.GetType(), out handler))
				handler(eventMessage);
            else
				this.registered.ThrowHandlerNotFound(eventMessage);
		}

		private void Register(Type messageType, Action<object> handler)
		{
			this.handlers[messageType] = handler;
		}
	}
}