namespace CommonDomain.Persistence.EventStore
{
	using System;
	using global::EventStore;

	public class EventStoreRepository : IRepository
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IStoreEvents eventStore;
		private readonly AggregateFactory factory;

		public EventStoreRepository(IUnitOfWork unitOfWork, IStoreEvents store, AggregateFactory factory)
		{
			this.unitOfWork = unitOfWork;
			this.eventStore = store;
			this.factory = factory;
		}

		public void Add(IAggregate aggregate)
		{
		}

		public TAggregate GetById<TAggregate>(Guid id, long version)
			where TAggregate : class, IAggregate
		{
			return null;
		}
	}
}