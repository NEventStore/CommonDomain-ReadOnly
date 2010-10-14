namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	public class BusAdapter<TEvent> : IPublishCommittedEvents
		where TEvent : class
	{
		private const string VersionPropertyName = "Version";

		private readonly Action<IEnumerable<TEvent>> publish;
		private readonly PropertyInfo property;

		public BusAdapter(Action<IEnumerable<TEvent>> publish)
		{
			this.publish = publish;
			this.property = typeof(TEvent).GetProperty(VersionPropertyName);
		}

		public void Publish(IEnumerable committedEvents)
		{
			this.publish(committedEvents.Cast<TEvent>().Where(x => x != null));
		}
		public void Publish(IEnumerable committedEvents, long startingVersion)
		{
			if (this.property == null)
				this.Publish(committedEvents);

			this.publish(this.GetVersionedEvents(committedEvents, startingVersion));
		}
		private IEnumerable<TEvent> GetVersionedEvents(IEnumerable committedEvents, long startingVersion)
		{
			foreach (var @event in committedEvents.Cast<TEvent>().Where(x => x != null))
			{
				this.property.SetValue(@event, startingVersion++, null);
				yield return @event;
			}
		}
	}
}