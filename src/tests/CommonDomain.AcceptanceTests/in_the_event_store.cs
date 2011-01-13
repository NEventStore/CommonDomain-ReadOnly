#pragma warning disable 169
// ReSharper disable InconsistentNaming

namespace CommonDomain.AcceptanceTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Core;
	using EventStore;
	using EventStore.Dispatcher;
	using EventStore.Persistence;
	using Machine.Specifications;
	using Persistence;
	using Persistence.EventStore;

	public class in_the_event_store
	{
		protected static IRepository repository;
		protected static ICollection<IDomainEvent> publishedEvents;
        protected static IStoreEvents eventStore;
        protected static InMemoryPersistenceEngine engine;

		Establish context = () =>
		{
			engine = new InMemoryPersistenceEngine();
			engine.Initialize();
		   
			publishedEvents = new List<IDomainEvent>();

			eventStore = new OptimisticEventStore(engine, new SynchronousDispatcher(new FakeBus(publishedEvents), engine));

			repository = new EventStoreRepository(eventStore, new AggregateFactory(), new ReflectionVersionStamper(), new ConflictDetector());
		};
	}

	internal class AggregateFactory : IConstructAggregates
	{
		public IAggregate Build(Type type, Guid id, IMemento snapshot)
		{
			// todo
			return Activator.CreateInstance(type, id) as IAggregate;
		}
	}

	public class FakeBus : IPublishMessages
	{
		readonly ICollection<IDomainEvent> domainEvents;

		public FakeBus(ICollection<IDomainEvent> domainEvents)
		{
			this.domainEvents = domainEvents;
		}

		public void Dispose()
		{
		}

		public void Publish(Commit commit)
		{
			commit.Events.ToList().ForEach(e => this.domainEvents.Add(e.Body as IDomainEvent));
		}
	}

	public interface IDomainEvent
	{
	}
}

// ReSharper enable InconsistentNaming
#pragma warning restore 169