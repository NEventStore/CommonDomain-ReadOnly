namespace CommonDomain.Persistence
{
	using System;

	public interface IIdentityMap
	{
		IdentityMapEntry GetById(Guid id);
		void Add(IdentityMapEntry entry);
		void Remove(Guid aggregateId);
	}
}