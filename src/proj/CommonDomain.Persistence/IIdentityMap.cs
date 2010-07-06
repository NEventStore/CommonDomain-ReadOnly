namespace CommonDomain.Persistence
{
	using System;

	public interface IIdentityMap
	{
		IAggregate GetById(Guid id);
		void Add(IAggregate aggregate);
		void Eject(Guid id);
	}
}