namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections;
	using global::EventStore;

	public class SagaEventStoreRepository : ISagaRepository
	{
		private readonly IStoreEvents eventStore;
		private readonly Action<ICollection> dispatcher;

		public SagaEventStoreRepository(IStoreEvents eventStore, Action<ICollection> dispatcher)
		{
			this.eventStore = eventStore;
			this.dispatcher = dispatcher;
		}

		public TSaga GetById<TSaga>(Guid id) where TSaga : class, ISaga, new()
		{
			var saga = new TSaga();

			var stream = this.eventStore.Read(id, 0);
			foreach (var @event in stream.Events)
				saga.Transition(@event);

			saga.ClearUncommittedEvents();
			saga.ClearUndispatchedMessages();

			return saga;
		}

		public void Save(ISaga saga)
		{
			var stream = BuildStream(saga);
			if (stream.Events.Count == 0)
				throw new NotSupportedException(ExceptionMessages.NoWork);

			this.Persist(stream);
			this.DispatchAndClearUncommittedMessages(saga);
		}
		private static UncommittedEventStream BuildStream(ISaga saga)
		{
			if (saga == null)
				throw new ArgumentNullException(
					ExceptionMessages.SagaArgument, ExceptionMessages.NullArgument);

			var events = saga.GetUncommittedEvents();
			return new UncommittedEventStream
			{
				Id = saga.Id,
				Type = saga.GetType(),
				ExpectedVersion = saga.Version - events.Count,
				Events = events
			};
		}
		private void Persist(UncommittedEventStream stream)
		{
			try
			{
				this.eventStore.Write(stream);
			}
			catch (StorageEngineException e)
			{
				throw new PersistenceException(e.Message, e);
			}
		}
		private void DispatchAndClearUncommittedMessages(ISaga saga)
		{
			this.dispatcher(saga.GetUndispatchedMessages());
			saga.ClearUndispatchedMessages();
			saga.ClearUncommittedEvents();
		}
	}
}