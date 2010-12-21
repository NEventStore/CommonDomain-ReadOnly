namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public interface IRepository
	{
		TAggregate GetById<TAggregate>(Guid id, long version) where TAggregate : class, IAggregate;
		void Save(IAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> headers);
	}
}