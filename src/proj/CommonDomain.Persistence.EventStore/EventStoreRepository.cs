namespace CommonDomain.Persistence.EventStore
{
	using System;
	using global::EventStore;

	public class EventStoreRepository : IRepository
	{
		private readonly IStoreEvents eventStore;
		private readonly IConstructAggregates factory;
		private readonly IPublishCommittedEvents publisher;
		private readonly IDetectConflicts conflictDetector;

		public EventStoreRepository(
			IStoreEvents eventStore,
			IConstructAggregates factory,
			IPublishCommittedEvents publisher,
			IDetectConflicts conflictDetector)
		{
			this.conflictDetector = conflictDetector;
			this.publisher = publisher;
			this.eventStore = eventStore;
			this.factory = factory;
		}

		public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
		{
			return this.GetById<TAggregate>(id, 0);
		}
		public TAggregate GetById<TAggregate>(Guid id, long version) where TAggregate : class, IAggregate
		{
			var stream = this.eventStore.Read(id, version);
			return this.BuildAggregate<TAggregate>(stream, version);
		}
		private TAggregate BuildAggregate<TAggregate>(CommittedEventStream stream, long version)
			where TAggregate : class, IAggregate
		{
			var aggregate = this.factory.Build(typeof(TAggregate), stream.Id, stream.Snapshot as IMemento);

			if (CanApplyEvents(aggregate, version))
				foreach (var @event in stream.Events)
					aggregate.ApplyEvent(@event);

			return aggregate as TAggregate;
		}
		private static bool CanApplyEvents(IAggregate aggregate, long version)
		{
			return version == 0 || aggregate.Version < version;
		}

		public void Save(IAggregate aggregate)
		{
			this.Save(aggregate, null, Guid.Empty);
		}
		public void Save(IAggregate aggregate, object command, Guid commandId)
		{
			var stream = BuildStream(aggregate, command, commandId);
			if (stream.Events.Count == 0)
				return;

			this.Persist(stream, stream.CommittedVersion + stream.Events.Count + 1);
			aggregate.ClearUncommittedEvents();
		}
		private static UncommittedEventStream BuildStream(IAggregate aggregate, object command, Guid commandId)
		{
			if (aggregate == null)
				throw new ArgumentNullException(
					ExceptionMessages.AggregateArgument, ExceptionMessages.NullArgument);

			var events = aggregate.GetUncommittedEvents();

			return new UncommittedEventStream
			{
				Id = aggregate.Id,
				Type = aggregate.GetType(),
				CommittedVersion = aggregate.Version - events.Count,
				CommandId = commandId,
				Command = command,
				Events = events
			};
		}
		private void Persist(UncommittedEventStream stream, long startingVersion)
		{
			try
			{
				this.eventStore.Write(stream);
				this.publisher.Publish(stream.Events, startingVersion);
			}
			catch (StorageEngineException e)
			{
				throw new PersistenceException(e.Message, e);
			}
			catch (StorageConstraintViolationException e)
			{
				throw new PersistenceException(e.Message, e);
			}
			catch (ConcurrencyException e)
			{
				if (this.conflictDetector.ConflictsWith(stream.Events, e.CommittedEvents))
					throw new ConflictingCommandException(ExceptionMessages.ConflictingCommand, e);

				stream.CommittedVersion += e.CommittedEvents.Count;
				startingVersion += e.CommittedEvents.Count;
				this.Persist(stream, startingVersion);
			}
			catch (DuplicateCommandException e)
			{
				this.publisher.Publish(e.CommittedEvents);
			}
		}
	}
}