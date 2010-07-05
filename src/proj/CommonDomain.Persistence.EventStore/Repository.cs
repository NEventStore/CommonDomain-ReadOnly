namespace CommonDomain.Persistence.EventStore
{
	using System;

	public class Repository : IRepository
	{
		public TAggregate GetById<TAggregate>(Guid id) where TAggregate : class, IAggregate
		{
			return default(TAggregate);
		}
		public void Add(IAggregate aggregate)
		{
		}
		public void Remove(IAggregate aggregate)
		{
		}
	}
}