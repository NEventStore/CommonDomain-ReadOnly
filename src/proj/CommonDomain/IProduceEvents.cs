namespace CommonDomain
{
	using System.Collections;

	public interface IProduceEvents
	{
		ICollection GetUncommittedEvents();
		void ClearUncommittedEvents();
	}
}