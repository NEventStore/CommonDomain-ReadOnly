namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class BusAdapter<TEvent> : IPublishCommittedEvents
		where TEvent : class
	{
		private readonly Action<IEnumerable<TEvent>> publish;

		public BusAdapter(Action<IEnumerable<TEvent>> publish)
		{
			this.publish = publish;
		}

		public void Publish(IEnumerable committedEvents)
		{
			this.publish(committedEvents.Cast<TEvent>().Where(x => x != null));
		}
	}
}