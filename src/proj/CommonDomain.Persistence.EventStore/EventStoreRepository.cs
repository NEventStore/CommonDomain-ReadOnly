namespace CommonDomain.Persistence.EventStore
{
	using System;
	using global::EventStore;

	public class EventStoreRepository : IRepository
	{
		private readonly IScopeCommandContext commandContext;
		private readonly IStoreEvents eventStore;
		private readonly AggregateFactory factory;
		private readonly IPublishCommittedEvents bus;
		private readonly IDetectConflicts conflictDetector;

		public EventStoreRepository(
			IScopeCommandContext commandContext,
			IStoreEvents eventStore,
			AggregateFactory factory,
			IPublishCommittedEvents bus,
			IDetectConflicts conflictDetector)
		{
			this.commandContext = commandContext;
			this.conflictDetector = conflictDetector;
			this.bus = bus;
			this.eventStore = eventStore;
			this.factory = factory;
		}

		public TAggregate GetById<TAggregate>(Guid id)
			where TAggregate : class, IAggregate
		{
			var context = this.commandContext.GetCurrent();
			var stream = this.eventStore.Read(id, context.Version);
			return this.BuildAggregate(stream, context) as TAggregate;
		}
		private IAggregate BuildAggregate(CommittedEventStream stream, CommandContext context)
		{
			var aggregate = this.factory(stream.Id, stream.Snapshot as IMomento);

			foreach (var @event in stream.Events)
				if (CanApplyEvent(aggregate, context))
					aggregate.ApplyEvent(@event);

			return aggregate;
		}
		private static bool CanApplyEvent(IAggregate aggregate, CommandContext current)
		{
			return current == null || current.Version == 0 || aggregate.Version < current.Version;
		}

		public void Save(IAggregate aggregate)
		{
			var stream = this.BuildStream(aggregate);
			if (stream.Events.Count == 0)
				throw new NotSupportedException(ExceptionMessages.NoWork);

			this.Persist(this.BuildStream(aggregate));
		}
		private UncommittedEventStream BuildStream(IAggregate aggregate)
		{
			var context = this.commandContext.GetCurrent();
			var events = aggregate.GetUncommittedEvents();

			return new UncommittedEventStream
			{
				Id = aggregate.Id,
				ExpectedVersion = aggregate.Version - events.Count,
				CommandId = context.Id,
				Command = context.Message,
				Events = events
			};
		}
		private void Persist(UncommittedEventStream stream)
		{
			try
			{
				this.eventStore.Write(stream);
				this.bus.Publish(stream.Events);
			}
			catch (StorageEngineException e)
			{
				throw new PersistenceException(e.Message, e);
			}
			catch (ConcurrencyException e)
			{
				if (this.conflictDetector.ConflictsWith(stream.Events, e.CommittedEvents))
					throw new ConflictingCommandException(ExceptionMessages.ConflictingCommand, e);

				stream.ExpectedVersion += e.CommittedEvents.Count;
				this.Persist(stream);
			}
			catch (DuplicateCommandException e)
			{
				this.bus.Publish(e.CommittedEvents);
			}
		}
	}
}