namespace CommonDomain.Persistence
{
	using System;

	public interface IUnitOfWork : IDisposable
	{
		void Register(Guid id, Action action);

		void Complete();
	}
}