namespace CommonDomain.Persistence.EventStore
{
	using System;
	using global::EventStore;

	public class EventStoreRepository : IRepository
	{
		private readonly IIdentityMap localMap;
		private readonly IUnitOfWork unitOfWork;
		private readonly IScopeCommandContext commandContext;
		private readonly IStoreEvents eventStore;
		private readonly AggregateFactory factory;

		public EventStoreRepository(
			IIdentityMap localMap,
			IUnitOfWork unitOfWork,
			IScopeCommandContext commandContext,
			IStoreEvents eventStore,
			AggregateFactory factory)
		{
			this.localMap = localMap;
			this.unitOfWork = unitOfWork;
			this.commandContext = commandContext;
			this.eventStore = eventStore;
			this.factory = factory;
		}

		public TAggregate GetById<TAggregate>(Guid id)
			where TAggregate : class, IAggregate
		{
			// if same cmd ID, it should return the exact same instance (per TAggregate) each time
			// without re-enlisting in the UoW.

			return default(TAggregate);
		}

		public void Add(IAggregate aggregate)
		{
			this.localMap.Add(new IdentityMapEntry(aggregate));
			var context = this.commandContext.GetCurrent();
			this.unitOfWork.Register(() => this.Persist(aggregate, context));
		}

		private void Persist(IAggregate aggregate, CommandContext context)
		{
			// guaranteed to have an identity map entry at this point...
			// which started on/before the earliest command version
		}
	}
}