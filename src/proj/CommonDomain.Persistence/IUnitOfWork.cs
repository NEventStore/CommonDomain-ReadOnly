namespace CommonDomain.Persistence
{
	using System;

	public interface IUnitOfWork : IDisposable
	{
		void Register(Action action);
		void Complete();
	}
}