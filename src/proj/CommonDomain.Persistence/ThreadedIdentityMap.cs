namespace CommonDomain.Persistence
{
	using System;
	using System.Collections.Generic;

	public class ThreadedIdentityMap : IIdentityMap
	{
		private readonly IDictionary<Guid, object> locks = new Dictionary<Guid, object>();
		private readonly IDictionary<Guid, IAggregate> map = new Dictionary<Guid, IAggregate>();

		public IAggregate GetById(Guid id)
		{
			IAggregate aggregate = null;
			this.LockAndExecute(id, () => this.map.TryGetValue(id, out aggregate));
			return aggregate;
		}

		public void Add(IAggregate aggregate)
		{
			this.LockAndExecute(aggregate.Id, () => this.map[aggregate.Id] = aggregate);
		}

		public void Eject(Guid id)
		{
			this.LockAndExecute(id, () => this.map.Remove(id));
		}

		private void LockAndExecute(Guid id, Action action)
		{
			lock (this.ObtainLock(id))
				action();
		}
		private object ObtainLock(Guid id)
		{
			object @lock;
			if (this.locks.TryGetValue(id, out @lock))
				return @lock;

			lock (this.locks)
				if (!this.locks.TryGetValue(id, out @lock))
					this.locks[id] = @lock = new object();

			return @lock;
		}
	}
}