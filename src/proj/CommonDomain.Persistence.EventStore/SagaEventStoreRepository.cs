namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections.Generic;
	using global::EventStore;
	using global::EventStore.Persistence;

	public class SagaEventStoreRepository : ISagaRepository
	{
		private readonly IDictionary<Guid, long> commitSequence = new Dictionary<Guid, long>();
		private readonly IStoreEvents eventStore;

		public SagaEventStoreRepository(IStoreEvents eventStore)
		{
			this.eventStore = eventStore;
		}

		public TSaga GetById<TSaga>(Guid sagaId) where TSaga : class, ISaga, new()
		{
			var stream = this.eventStore.ReadUntil(sagaId, 0);
			this.commitSequence[sagaId] = stream.CommitSequence;

			return BuildSaga<TSaga>(stream);
		}
		private static TSaga BuildSaga<TSaga>(CommittedEventStream stream) where TSaga : class, ISaga, new()
		{
			var saga = new TSaga();
			foreach (var @event in stream.Events)
				saga.Transition(@event);

			saga.ClearUncommittedEvents();
			saga.ClearUndispatchedMessages();

			return saga;
		}

		public void Save(ISaga saga, Guid commitId, Action<IDictionary<string, object>> headers)
		{
			var attempt = this.BuildAttempt(saga, commitId);
			if (attempt.Events.Count == 0)
				throw new NotSupportedException(ExceptionMessages.NoWork);

			if (headers != null)
				headers(attempt.Headers);

			this.Persist(attempt);

			saga.ClearUndispatchedMessages();
			saga.ClearUncommittedEvents();
		}
		private CommitAttempt BuildAttempt(ISaga saga, Guid commitId)
		{
			if (saga == null)
				throw new ArgumentNullException("saga", ExceptionMessages.NullArgument);

			var events = saga.GetUncommittedEvents();

			var attempt = new CommitAttempt
			{
				StreamId = saga.Id,
				StreamName = saga.GetType().FullName,
				CommitId = commitId,
				StreamRevision = saga.Version,
				PreviousCommitSequence = this.commitSequence[saga.Id]
			};

			foreach (var @event in events)
				attempt.Events.Add(new EventMessage { Body = @event });

			return attempt;
		}
		private void Persist(CommitAttempt stream)
		{
			try
			{
				// any optimistic concurrency exceptions means we should try again
				this.eventStore.Write(stream);
			}
			catch (PersistenceEngineException e)
			{
				throw new PersistenceException(e.Message, e);
			}
			catch (DuplicateCommitException)
			{
			}
		}
	}
}