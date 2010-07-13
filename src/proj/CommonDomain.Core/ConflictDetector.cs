namespace CommonDomain.Core
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;

	public class ConflictDetector : IDetectConflicts
	{
		// this contains some lambda magic that effectively allows us to cast
		// between TEvent (committed or uncommitted) and object in a completely type-safe way.
		private readonly IDictionary<Type, IDictionary<Type, Func<object, object, bool>>> actions =
			new Dictionary<Type, IDictionary<Type, Func<object, object, bool>>>();

		public void Register<TUncommitted, TCommitted>(Func<TUncommitted, TCommitted, bool> handler)
			where TUncommitted : class
			where TCommitted : class
		{
			IDictionary<Type, Func<object, object, bool>> inner;
			if (!this.actions.TryGetValue(typeof(TUncommitted), out inner))
				this.actions[typeof(TUncommitted)] = inner = new Dictionary<Type, Func<object, object, bool>>();

			inner[typeof(TCommitted)] =
				(uncommitted, committed) => handler(uncommitted as TUncommitted, committed as TCommitted);
		}
		public bool ConflictsWith(ICollection uncommittedEvents, ICollection committedEvents)
		{
			return (from object uncommitted in uncommittedEvents
			        from object committed in committedEvents
			        where this.Conflicts(uncommitted, committed)
			        select uncommittedEvents).Any();
		}

		private bool Conflicts(object uncommitted, object committed)
		{
			return this.actions[uncommitted.GetType()][committed.GetType()](uncommitted, committed);
		}
	}
}