namespace CommonDomain.Persistence
{
	using System;

	public interface IRepository
	{
		TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate;
		void Add(IAggregate aggregate);
	}
}