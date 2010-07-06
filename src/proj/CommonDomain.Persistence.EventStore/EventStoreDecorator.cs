namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections;
	using global::EventStore;

	public class EventStoreDecorator : IStoreEvents
	{
		private readonly IStoreEvents eventStore;
		private readonly IIdentityMap identityMap;
		private readonly IConflictWith conflictDetector;

		public EventStoreDecorator(
			IStoreEvents eventStore, IIdentityMap identityMap, IConflictWith conflictDetector)
		{
			this.eventStore = eventStore;
			this.identityMap = identityMap;
			this.conflictDetector = conflictDetector;
		}

		public CommittedEventStream Read(Guid id)
		{
			try
			{
				return this.eventStore.Read(id);
			}
			catch (StorageEngineException e)
			{
				throw new PersistenceException(e.Message, e);
			}
		}

		public void Write(UncommittedEventStream stream)
		{
			try
			{
				this.eventStore.Write(stream);
			}
			catch (ConcurrencyException e)
			{
				this.ThrowOnConflict(e, stream);
			}
			catch (DuplicateCommandException e)
			{
				// TODO: republish
			}
			catch (StorageEngineException e)
			{
				throw new PersistenceException(e.Message, e);
			}
		}
		private void ThrowOnConflict(ConcurrencyException exception, UncommittedEventStream uncommitted)
		{
			foreach (var @event in uncommitted.Events)
			{
				if (!this.EventsConflict(@event, exception.CommittedEvents))
					continue;

				// events don't reconcile; TODO: how do we notify the client/caller of a stale command?
				this.identityMap.Eject(uncommitted.Id);
				throw new ConflictingCommandException(exception.Message, exception);
			}
		}
		private bool EventsConflict(object uncommittedEvent, ICollection committedEvents)
		{
			foreach (var committedEvent in committedEvents)
				if (this.conflictDetector.ConflictsWith(uncommittedEvent, committedEvent))
					return true;

			return false;
		}
	}
}