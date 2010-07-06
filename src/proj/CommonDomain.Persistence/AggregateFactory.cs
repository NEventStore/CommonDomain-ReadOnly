namespace CommonDomain.Persistence
{
	using System;

	public delegate IAggregate AggregateFactory(Guid id, object snapshot);
}