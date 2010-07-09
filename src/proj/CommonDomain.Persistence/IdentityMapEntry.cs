namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public class IdentityMapEntry
	{
		public Guid AggregateId { get; set; }
		public long Version { get; set; }
		public IList<object> Events { get; set; }
		public IMomento Snapshot { get; set; }
	}
}