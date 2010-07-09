namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public class SimpleUnitOfWork : IUnitOfWork
	{
		private readonly IDictionary<Guid, Action> actions = new Dictionary<Guid, Action>();

		public void Register(Guid id, Action action)
		{
			this.actions[id] = action;
		}

		public void Complete()
		{
			foreach (var action in this.actions.Values)
				action();
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(true);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				this.actions.Clear();
		}
	}
}