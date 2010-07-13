namespace CommonDomain
{
	using System;
	using System.Collections;

	public interface IDetectConflicts
	{
		void Register<TUncommitted, TCommitted>(Func<TUncommitted, TCommitted, bool> handler)
			where TUncommitted : class
			where TCommitted : class;

		bool ConflictsWith(ICollection uncommittedEvents, ICollection committedEvents);
	}
}