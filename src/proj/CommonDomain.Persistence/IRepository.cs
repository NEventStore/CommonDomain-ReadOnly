namespace CommonDomain.Persistence
{
	using System;

	public interface IRepository
	{
		TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate;
		TAggregate GetById<TAggregate>(Guid id, long version) where TAggregate : class, IAggregate;

		void Save(IAggregate aggregate);
		void Save(IAggregate aggregate, object command, Guid commandId);
	}
}