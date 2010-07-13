namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public class IdentityMapEntry
	{
		public IdentityMapEntry(IAggregate aggregate)
		{
			this.Id = aggregate.Id;
			this.Events = (ICollection<object>)aggregate.GetUncommittedEvents();
			this.Version = aggregate.Version + this.Events.Count;
		}
		public IdentityMapEntry()
		{
			this.Events = new LinkedList<object>();
		}

		//// public IAggregate Aggregate { get; set; }
		public Guid Id { get; set; }
		public long Version { get; set; }
		public ICollection<object> Events { get; set; }
		public IMomento Snapshot { get; set; }
	}
}