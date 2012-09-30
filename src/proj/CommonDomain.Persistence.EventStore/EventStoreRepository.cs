namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using global::EventStore;
	using global::EventStore.Persistence;

	public class EventStoreRepository : IRepository
	{
		private const string AggregateTypeHeader = "AggregateType";
		private readonly IStoreEvents eventStore;
		private readonly IConstructAggregates factory;
		private readonly IDetectConflicts conflictDetector;

		public EventStoreRepository(
			IStoreEvents eventStore,
			IConstructAggregates factory,
			IDetectConflicts conflictDetector)
		{
			this.eventStore = eventStore;
			this.factory = factory;
			this.conflictDetector = conflictDetector;
		}

        public virtual TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
        {
            return GetById<TAggregate>(id, int.MaxValue);
        }

	    public virtual TAggregate GetById<TAggregate>(Guid id, int versionToLoad) where TAggregate : class, IAggregate
		{
			var snapshot = GetSnapshot(id, versionToLoad);
            using (var stream = OpenStream(id, versionToLoad, snapshot))
            {
                var aggregate = GetAggregate<TAggregate>(snapshot, stream);

                ApplyEventsToAggregate(versionToLoad, stream, aggregate);

                return aggregate as TAggregate;
            }
		}
		private static void ApplyEventsToAggregate(int versionToLoad, IEventStream stream, IAggregate aggregate)
		{
			if (versionToLoad == 0 || aggregate.Version < versionToLoad)
				foreach (var @event in stream.CommittedEvents.Select(x => x.Body))
					aggregate.ApplyEvent(@event);
		}
		private IAggregate GetAggregate<TAggregate>(Snapshot snapshot, IEventStream stream)
		{
			var memento = snapshot == null ? null : snapshot.Payload as IMemento;
			return factory.Build(typeof(TAggregate), stream.StreamId, memento, stream.CommittedHeaders);
		}
		private Snapshot GetSnapshot(Guid id, int version)
		{
			return eventStore.Advanced.GetSnapshot(id, version);
		}
		private IEventStream OpenStream(Guid id, int version, Snapshot snapshot)
		{
			return snapshot == null
				? eventStore.OpenStream(id, 0, version)
				: eventStore.OpenStream(snapshot, version);
		}

		public virtual void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
		{
			var headers = PrepareHeaders(aggregate, updateHeaders);
			while (true)
			{
                using (var stream = PrepareStream(aggregate, headers))
                {
                    var commitEventCount = stream.CommittedEvents.Count;

                    try
                    {
                        stream.CommitChanges(commitId);
                        aggregate.ClearUncommittedEvents();
                        return;
                    }
                    catch (DuplicateCommitException)
                    {
                        stream.ClearChanges();
                        return;
                    }
                    catch (ConcurrencyException e)
                    {
                        if (ThrowOnConflict(stream, commitEventCount))
                            throw new ConflictingCommandException(e.Message, e);

                        stream.ClearChanges();
                    }
                    catch (StorageException e)
                    {
                        throw new PersistenceException(e.Message, e);
                    }
                }
			}
		}
		private IEventStream PrepareStream(IAggregate aggregate, Dictionary<string, object> headers)
		{
			IEventStream stream = eventStore.OpenStream(aggregate.Id, 0, int.MaxValue);

			foreach (var item in headers)
				stream.UncommittedHeaders[item.Key] = item.Value;

			aggregate.GetUncommittedEvents()
				.Cast<object>()
				.Select(x => new EventMessage { Body = x })
				.ToList()
				.ForEach(stream.Add);

			return stream;
		}
		private static Dictionary<string, object> PrepareHeaders(IAggregate aggregate, Action<IDictionary<string, object>> updateHeaders)
		{
			var headers = new Dictionary<string, object>();

			headers[AggregateTypeHeader] = aggregate.GetType().FullName;
			if (updateHeaders != null)
				updateHeaders(headers);

			return headers;
		}
		private bool ThrowOnConflict(IEventStream stream, int skip)
		{
			var committed = stream.CommittedEvents.Skip(skip).Select(x => x.Body);
			var uncommitted = stream.UncommittedEvents.Select(x => x.Body);
			return conflictDetector.ConflictsWith(uncommitted, committed);
		}
	}
}