namespace CommonDomain.Persistence
{
	using System.Collections;

	public interface IPublishCommittedEvents
	{
		void Publish(ICollection committedEvents);
	}
}