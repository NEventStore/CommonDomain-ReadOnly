namespace CommonDomain.Persistence.EventStore
{
	using System;
	using global::EventStore;

	public class Repository : IRepository
	{
		private readonly IStoreEvents eventStore;
		private readonly IIdentityMap identityMap;
		private readonly IUnitOfWork unitOfWork;
		private readonly AggregateFactory buildAggregate;

		public Repository(
			IStoreEvents eventStore,
			IIdentityMap identityMap,
			IUnitOfWork unitOfWork,
			AggregateFactory buildAggregate)
		{
			this.eventStore = eventStore;
			this.identityMap = identityMap;
			this.unitOfWork = unitOfWork;
			this.buildAggregate = buildAggregate;
		}

		public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
		{
			var aggregate = this.GetById(id);
			this.unitOfWork.Register(aggregate.Id, () => this.Persist(aggregate));
			return aggregate as TAggregate;
		}
		private IAggregate GetById(Guid id)
		{
			var mapped = this.GetFromIdentityMap(id);
			if (mapped != null)
				return mapped;

			var persisted = this.GetFromEventStore(id);
			this.identityMap.Add(persisted);
			return persisted;
		}
		private IAggregate GetFromIdentityMap(Guid id)
		{
			return this.identityMap.GetById(id);
		}
		private IAggregate GetFromEventStore(Guid id)
		{
			var stream = this.eventStore.Read(id, 0);

			var aggregate = this.buildAggregate(id, stream.Snapshot);
			foreach (var @event in stream.Events)
				aggregate.ApplyEvent(@event);

			return aggregate;
		}

		public void Add(IAggregate aggregate)
		{
			this.unitOfWork.Register(aggregate.Id, () => this.Persist(aggregate));
			this.identityMap.Add(aggregate);
		}
		public void Remove(IAggregate aggregate)
		{
			throw new NotSupportedException("Removing aggregates is not supported.");
		}

		private void Persist(IAggregate aggregate)
		{
			if (null == aggregate || aggregate.Id == Guid.Empty)
				throw new ArgumentException("The aggregate has not been populated.");

			var uncommitted = BuildEventStream(aggregate);
			this.eventStore.Write(uncommitted);
			aggregate.ClearUncommittedEvents(); // TODO: publish
		}
		private static UncommittedEventStream BuildEventStream(IAggregate aggregate)
		{
			return new UncommittedEventStream
			{
				Id = aggregate.Id,
				Type = aggregate.GetType(),
				//// Command = null, // TODO: get from thread storage?
				//// CommandId = Guid.Empty, // TODO: get from thread storage?
				Events = aggregate.GetUncommittedEvents(),
				Snapshot = null // will be done by another process
			};
		}
	}
}