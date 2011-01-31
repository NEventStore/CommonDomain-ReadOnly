namespace CommonDomain.Persistence.EventStore
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using global::EventStore;
	using global::EventStore.Persistence;

	public class SagaEventStoreRepository : ISagaRepository, IDisposable
	{
		private const string SagaTypeHeader = "SagaType";
		private const string UndispatchedMessageHeader = "UndispatchedMessage.";
		private readonly IDictionary<Guid, IEventStream> streams = new Dictionary<Guid, IEventStream>();
		private readonly IStoreEvents eventStore;

		public SagaEventStoreRepository(IStoreEvents eventStore)
		{
			this.eventStore = eventStore;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			lock (this.streams)
			{
				foreach (var stream in this.streams)
					stream.Value.Dispose();

				this.streams.Clear();
			}
		}

		public TSaga GetById<TSaga>(Guid sagaId) where TSaga : class, ISaga, new()
		{
			return BuildSaga<TSaga>(this.OpenStream(sagaId));
		}
		private IEventStream OpenStream(Guid sagaId)
		{
			IEventStream stream;
			if (this.streams.TryGetValue(sagaId, out stream))
				return stream;

			try
			{
				stream = this.eventStore.OpenStream(sagaId, 0, int.MaxValue);
			}
			catch (StreamNotFoundException)
			{
				stream = this.eventStore.CreateStream(sagaId);
			}

			return this.streams[sagaId] = stream;
		}

		private static TSaga BuildSaga<TSaga>(IEventStream stream) where TSaga : class, ISaga, new()
		{
			var saga = new TSaga();
			foreach (var @event in stream.CommittedEvents.Select(x => x.Body))
				saga.Transition(@event);

			saga.ClearUncommittedEvents();
			saga.ClearUndispatchedMessages();

			return saga;
		}

		public void Save(ISaga saga, Guid commitId, Action<IDictionary<string, object>> updateHeaders)
		{
			if (saga == null)
				throw new ArgumentNullException("saga", ExceptionMessages.NullArgument);

			var stream = this.PrepareStream(saga);
			var headers = PrepareHeaders(saga, updateHeaders);

			Persist(stream, commitId, headers);

			saga.ClearUncommittedEvents();
			saga.ClearUndispatchedMessages();
		}
		private IEventStream PrepareStream(ISaga saga)
		{
			IEventStream stream;
			if (!this.streams.TryGetValue(saga.Id, out stream))
				this.streams[saga.Id] = stream = this.eventStore.CreateStream(saga.Id);

			foreach (var @event in saga.GetUncommittedEvents())
				stream.Add(@event);

			return stream;
		}
		private static Dictionary<string, object> PrepareHeaders(ISaga saga, Action<IDictionary<string, object>> updateHeaders)
		{
			var headers = new Dictionary<string, object>();

			headers[SagaTypeHeader] = saga.GetType().FullName;
			if (updateHeaders != null)
				updateHeaders(headers);

			var i = 0;
			foreach (var command in saga.GetUndispatchedMessages())
				headers[UndispatchedMessageHeader + i++] = command;

			return headers;
		}
		private static void Persist(IEventStream stream, Guid commitId, Dictionary<string, object> headers)
		{
			try
			{
				stream.CommitChanges(commitId, headers);
			}
			catch (DuplicateCommitException)
			{
			}
			catch (StorageException e)
			{
				throw new PersistenceException(e.Message, e);
			}	
		}
	}
}