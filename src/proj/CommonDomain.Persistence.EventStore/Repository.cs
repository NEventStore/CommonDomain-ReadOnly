namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections.Generic;
	using global::EventStore;

	public class Repository : IRepository
	{
		private readonly IStoreEvents eventStore;
		private IDictionary<Guid, IAggregate> identityMap;
		private readonly Func<Guid, IAggregate> factory;

		public Repository(IStoreEvents eventStore, IDictionary<Guid, IAggregate> identityMap, Func<Guid, IAggregate> factory)
		{
			this.eventStore = eventStore;
			this.factory = factory;
			this.identityMap = identityMap;
		}

		public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
		{
			IAggregate aggregate = null;
			if (this.identityMap.TryGetValue(id, out aggregate))
				return aggregate as TAggregate;

			aggregate = this.factory(id);

			this.identityMap[id] = aggregate; // TODO: identity map must be thread safe...
			return aggregate as TAggregate;
		}
		public void Add(IAggregate aggregate)
		{
		}
		public void Remove(IAggregate aggregate)
		{
		}
	}
}