namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public interface IRepository
  {
    IAggregate GetById(Type aggregateType, Guid id);
    IAggregate GetById(Type aggregateType, Guid id, int version);
		void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders);
	}
}