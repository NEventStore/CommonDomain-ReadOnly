namespace CommonDomain.Persistence
{
	using System;

	public interface ISagaRepository
	{
		TSaga GetById<TSaga>(Guid id) where TSaga : class, ISaga, new();
		void Save(ISaga saga);
	}
}