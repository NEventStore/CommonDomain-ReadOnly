namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using global::EventStore;

	public class EventStoreRepository : IRepository
	{
		private readonly IDictionary<Guid, IdentityMapEntry> identityMap = new Dictionary<Guid, IdentityMapEntry>();
		private readonly IHandleCommandContext commandContext;
		private readonly IStoreEvents eventStore;
		private readonly AggregateFactory factory;

		public EventStoreRepository(IHandleCommandContext commandContext, IStoreEvents eventStore, AggregateFactory factory)
		{
			this.eventStore = eventStore;
			this.commandContext = commandContext;
			this.factory = factory;
		}

		public TAggregate GetById<TAggregate>(Guid id)
			where TAggregate : class, IAggregate
		{
			var context = this.commandContext.GetCurrent();
			var entry = this.GetOrCreateIdentityMapEntry(id, () => this.LoadFromStorage(id, context.Version));
			return this.BuildAggregate(entry, context.Version) as TAggregate;
		}
		private IdentityMapEntry GetOrCreateIdentityMapEntry(Guid id, Func<IdentityMapEntry> create)
		{
			// TODO: if command version is *before* the (entry.Version - entry.CommittedEvents), eject the entry from the map
			// this ensures that we will always be able to evaluate commands from the past correctly for conflicts

			IdentityMapEntry entry;
			if (!this.identityMap.TryGetValue(id, out entry))
				this.identityMap[id] = entry = create();

			return entry;
		}
		private IdentityMapEntry LoadFromStorage(Guid id, long maxStartingVersion)
		{
			var stream = this.eventStore.Read(id, maxStartingVersion);
			return new IdentityMapEntry
			{
				Id = stream.Id,
				Version = stream.Version,
				CommittedEvents = stream.Events as ICollection<object>,
				Snapshot = stream.Snapshot as IMomento
			};
		}
		private IAggregate BuildAggregate(IdentityMapEntry entry, long stoppingVersion)
		{
			var aggregate = this.factory(entry.Id, entry.Snapshot);

			var version = aggregate.Version;
			foreach (var @event in entry.CommittedEvents)
			{
				if (version > 0 && ++version > stoppingVersion)
					break;

				aggregate.ApplyEvent(@event);
			}

			return aggregate;
		}

		public void Save(IAggregate aggregate)
		{
			var entry = this.GetOrCreateIdentityMapEntry(aggregate.Id, () => new IdentityMapEntry());
			var uncommitted = this.BuildStreamToCommit(aggregate);
			this.Commit(entry, uncommitted); // TODO: publish
		}
		private UncommittedEventStream BuildStreamToCommit(IAggregate aggregate)
		{
			var context = this.commandContext.GetCurrent();
			var uncommitted = aggregate.GetUncommittedEvents();

			return new UncommittedEventStream
			{
				Id = aggregate.Id,
				Type = aggregate.GetType(),
				ExpectedVersion = aggregate.Version - uncommitted.Count,
				CommandId = context.Id,
				Command = context.Message,
				Events = uncommitted,
				Snapshot = null
			};
		}
		private void Commit(IdentityMapEntry entry, UncommittedEventStream stream)
		{
			this.ThrowOnConflict(entry, stream);

			try
			{
				this.eventStore.Write(stream);
			}
			catch (ConcurrencyException e)
			{
				AppendCommittedEvents(entry, e.CommittedEvents);
				this.Commit(entry, stream);
			}
			catch (DuplicateCommandException e)
			{
				throw; // TODO
			}
			catch (StorageEngineException e)
			{
				throw new PersistenceException(e.Message, e);
			}

			AppendCommittedEvents(entry, stream.Events);
		}
		private void ThrowOnConflict(IdentityMapEntry entry, UncommittedEventStream stream)
		{
			var startOfIdMapEntryVersion = entry.Version - entry.CommittedEvents.Count;

			if (startOfIdMapEntryVersion > stream.ExpectedVersion)
				throw new Exception(); // can't compare (this will be done when getting the map entry)

			var committedToCompare = entry.CommittedEvents.Skip(0).Take(0).ToArray(); // TODO

			foreach (var uncommitted in stream.Events)
			{
				foreach (var committed in committedToCompare)
				{
					if (this.ConflictsWith(uncommitted, committed))
						throw new Exception(); // TODO
				}
			}
		}
		private bool ConflictsWith(object uncommitted, object committed)
		{
			// TODO: add comparison object
			return true;
		}
		private static void AppendCommittedEvents(IdentityMapEntry entry, ICollection newlyCommitted)
		{
			foreach (var @event in newlyCommitted)
				entry.CommittedEvents.Add(@event);

			entry.Version += newlyCommitted.Count;
		}
	}
}