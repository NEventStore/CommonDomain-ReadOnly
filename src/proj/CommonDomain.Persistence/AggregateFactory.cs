namespace CommonDomain.Persistence
{
	using System;

	public delegate IAggregate AggregateFactory(Type type, Guid id, IMemento snapshot);
}