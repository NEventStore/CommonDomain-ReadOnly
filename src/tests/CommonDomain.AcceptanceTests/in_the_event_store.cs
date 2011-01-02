using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonDomain.AcceptanceTests
{
    using Core;
    using EventStore;
    using EventStore.Dispatcher;
    using EventStore.Persistence;
    using EventStore.Persistence.InMemoryPersistence;
    using Machine.Specifications;
    using Persistence;
    using Persistence.EventStore;

    public class in_the_event_store
    {
        protected static IRepository _repository;
        protected static ICollection<IDomainEvent> _publishedEvents;

        Establish context = () =>
        {
            var engine = new InMemoryPersistenceEngine();
            engine.Initialize();
           
            _publishedEvents = new List<IDomainEvent>();

            var eventStore = new OptimisticEventStore(engine, new SynchronousDispatcher(new FakeBus(_publishedEvents), engine));

            _repository = new EventStoreRepository(eventStore, new AggregateFactory(), new ReflectionVersionStamper(), new ConflictDetector());
        
        };

        Cleanup everything = () =>
        {
        };

    }

    internal class AggregateFactory : IConstructAggregates
    {
        public IAggregate Build(Type type, Guid id, IMemento snapshot)
        {
            //todo
            return Activator.CreateInstance(type,id) as IAggregate;
        }
    }


    public class FakeBus : IPublishMessages
    {
        readonly ICollection<IDomainEvent> _domainEvents;

        public FakeBus(ICollection<IDomainEvent> domainEvents)
        {
            _domainEvents = domainEvents;
        }

        public void Dispose()
        {
        }

        public void Publish(Commit commit)
        {
            commit.Events.ToList().ForEach(e => _domainEvents.Add(e.Body as IDomainEvent));
        }
    }

    public interface IDomainEvent
    {
    }
}
