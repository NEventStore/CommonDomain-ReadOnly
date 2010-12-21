namespace CommonDomain
{
	using System.Collections;

	public interface IDetectConflicts
	{
		void Register<TUncommitted, TCommitted>(ConflictDelegate handler)
			where TUncommitted : class
			where TCommitted : class;

		bool ConflictsWith(ICollection uncommittedEvents, ICollection committedEvents);
	}

	public delegate bool ConflictDelegate(object uncommitted, object committed);
}