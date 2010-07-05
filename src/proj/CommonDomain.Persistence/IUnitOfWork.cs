namespace CommonDomain.Persistence
{
	using System;

	public interface IUnitOfWork : IDisposable
	{
		void RegisterNew(IAggregate aggregate);
		void RegisterDirty(IAggregate aggregate);
		void RegisterDeleted(IAggregate aggregate);

		void Complete();
	}
}