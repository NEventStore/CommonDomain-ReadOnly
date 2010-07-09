namespace CommonDomain.Persistence
{
	using System;

	public interface IRepository
	{
		void Add(IAggregate aggregate);
		TAggregate GetById<TAggregate>(Guid id, long version)
			where TAggregate : class, IAggregate;
	}
}