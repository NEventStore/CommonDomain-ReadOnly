namespace CommonDomain
{
	using System.Collections;

	public interface IDetectConflicts
	{
		bool ConflictsWith(ICollection uncommittedEvents, ICollection committedEvents);
	}
}