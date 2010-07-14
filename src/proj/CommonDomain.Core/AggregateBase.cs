namespace CommonDomain.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public abstract class AggregateBase<TEvent> : IAggregate, IEquatable<IAggregate>
		where TEvent : class
	{
		private readonly IDictionary<Type, Action<TEvent>> handlers = new Dictionary<Type, Action<TEvent>>();
		private readonly ICollection<TEvent> uncommittedEvents = new LinkedList<TEvent>();

		public Guid Id { get; protected set; }
		public long Version { get; protected set; }

		protected void Register<TRegisteredEvent>(Action<TRegisteredEvent> handler)
			where TRegisteredEvent : class, TEvent
		{
			this.handlers[typeof(TRegisteredEvent)] = @event => handler(@event as TRegisteredEvent);
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
			var snapshot = this.GetSnapshot();
			snapshot.Id = this.Id;
			snapshot.Version = this.Version;
			return snapshot;
		}
		protected abstract IMomento GetSnapshot();

		public override int GetHashCode()
		{
			return this.Id.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			return this.Equals(obj as IAggregate);
		}
		public virtual bool Equals(IAggregate other)
		{
			return null != other && other.Id == this.Id;
		}
	}
}