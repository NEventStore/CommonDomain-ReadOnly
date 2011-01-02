namespace CommonDomain.AcceptanceTests
{
    using System;
    using Core;
    using EventStore;
    using Machine.Specifications;

    [Subject("Persistence")]
    public class When_an_aggregate_is_persisted: in_the_event_store
    {
        static Guid id = Guid.NewGuid();
        static TestAggregate aggregate;

        Establish context = () => aggregate = new TestAggregate(id,"Some name");
            

        Because of = () =>
            _repository.Save(aggregate, Guid.NewGuid(), null);

        It should_be_returned_when_calling_get_by_id = () =>
                                                     _repository.GetById<TestAggregate>(id, 0).ShouldEqual(aggregate);
    }

    [Subject("Persistence")]
    public class When_an_aggregate_is_updated : in_the_event_store
    {
        static Guid id = Guid.NewGuid();
        static string newName = "New name";

        Establish context = () => _repository.Save(new TestAggregate(id, "Some name"), Guid.NewGuid(), null);


        Because of = () =>
            {
                var aggregate = _repository.GetById<TestAggregate>(id, 0);
                aggregate.ChangeName(newName);

                _repository.Save(aggregate,Guid.NewGuid(),null);
            };
          
        It should_the_version_number_should_increase = () =>
                                                     _repository.GetById<TestAggregate>(id, 0).Version.ShouldEqual(2);
        It should_update_the_aggregate = () =>
                                                     _repository.GetById<TestAggregate>(id, 0).Name.ShouldEqual(newName);
    }


    public class TestAggregate : AggregateBase<IDomainEvent>
    {
        public string Name { get; set; }
        public TestAggregate(Guid id)
        {
            Register<TestAggregateCreatedEvent>(Apply);
            Register<NameChangedEvent>(Apply);
            Id = id;
        }

        public TestAggregate(Guid id,string name):this(id)
        {
            this.RaiseEvent(new TestAggregateCreatedEvent
                                {
                                    Name = name
                                });
        }
        private void Apply(TestAggregateCreatedEvent @event)
        {
            Name = @event.Name;
        }
        private void Apply(NameChangedEvent @event)
        {
            Name = @event.Name;
        }

        public void ChangeName(string newName)
        {
            this.RaiseEvent(new NameChangedEvent{Name = newName});
        }
    }

    public class NameChangedEvent : IDomainEvent
    {
        public string Name { get; set; }
    }

    public class TestAggregateCreatedEvent : IDomainEvent
    {
        public string Name { get; set; }
    }
}