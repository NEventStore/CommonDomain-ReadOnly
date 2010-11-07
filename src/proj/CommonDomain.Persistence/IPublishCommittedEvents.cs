namespace CommonDomain.Persistence
{
	using System.Collections;

	public interface IPublishCommittedEvents
	{
		void Publish(IEnumerable committedEvents);
	}
}