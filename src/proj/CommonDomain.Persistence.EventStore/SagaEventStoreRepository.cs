namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections.Generic;
	using global::EventStore;
	using global::EventStore.Persistence;

	public class SagaEventStoreRepository : ISagaRepository
	{
		private const string SagaTypeHeader = "SagaType";
		private const string UndispatchedMessageHeader = "UndispatchedMessage.";
		private readonly IDictionary<Guid, int> commitSequence = new Dictionary<Guid, int>();
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

			attempt.Headers[SagaTypeHeader] = saga.GetType().FullName;

			this.Persist(attempt);

			saga.ClearUndispatchedMessages();
			saga.ClearUncommittedEvents();
		}
		private CommitAttempt BuildAttempt(ISaga saga, Guid commitId)
		{
			if (saga == null)
				throw new ArgumentNullException("saga", ExceptionMessages.NullArgument);

			var attempt = new CommitAttempt
			{
				StreamId = saga.Id,
				StreamRevision = saga.Version,
				CommitId = commitId
			};

			int previousCommitSequence;
			if (this.commitSequence.TryGetValue(attempt.StreamId, out previousCommitSequence))
				attempt.PreviousCommitSequence = previousCommitSequence;

			foreach (var @event in saga.GetUncommittedEvents())
				attempt.Events.Add(new EventMessage { Body = @event });

			var i = 0;
			foreach (var command in saga.GetUndispatchedMessages())
				attempt.Headers[UndispatchedMessageHeader + i++] = command;

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