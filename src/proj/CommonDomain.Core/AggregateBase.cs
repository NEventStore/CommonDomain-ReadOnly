namespace CommonDomain.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public abstract class AggregateBase<TEvent> : IAggregate
		where TEvent : class
	{
		private readonly IDictionary<Type, Action<TEvent>> handlers = new Dictionary<Type, Action<TEvent>>();
		private readonly ICollection<TEvent> uncommittedEvents = new LinkedList<TEvent>();

		public Guid Id { get; protected set; }
		public long Version { get; protected set; }

		protected void Register<TEventType>(Action<TEventType> handler)
			where TEventType : class, TEvent
		{
			this.handlers[typeof(TEventType)] = @event => handler(@event as TEventType);
		}

		protected void RaiseEvent(TEvent @event)
		{
			((IAggregate)this).ApplyEvent(@event);
			this.uncommittedEvents.Add(@event);
		}
		void IAggregate.ApplyEvent(object @event)
		{
			this.handlers[@event.GetType()](@event as TEvent);
			this.Version++;
		}
		ICollection IAggregate.GetUncommittedEvents()
		{
			return (ICollection)this.uncommittedEvents;
		}
		void IAggregate.ClearUncommittedEvents()
		{
			this.uncommittedEvents.Clear();
		}

		IMomento IAggregate.GetSnapshot()
		{
			var snapshot = this.TakeSnapshot();
			snapshot.Id = this.Id;
			snapshot.Version = this.Version;
			return snapshot;
		}
		protected abstract IMomento TakeSnapshot();
	}
}