namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections.Generic;

	public class IdentityMapEntry
	{
		public IdentityMapEntry()
		{
			this.CommittedEvents = new LinkedList<object>();
		}

		public Guid Id { get; set; }
		public long Version { get; set; }
		public ICollection<object> CommittedEvents { get; set; }
		public IMomento Snapshot { get; set; }
	}
}