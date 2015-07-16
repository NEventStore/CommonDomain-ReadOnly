using System.Collections.Generic;

namespace CommonDomain.Persistence
{
	using System;

	public interface IConstructAggregates
	{
		IAggregate Build(Type type, Guid id, IMemento snapshot, IDictionary<string, object> headers);
	}
}