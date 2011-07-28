#pragma warning disable 169
// ReSharper disable InconsistentNaming

namespace CommonDomain.AcceptanceTests
{
	using System;
	using System.Collections.Generic;
	using Core;
	using EventStore;
	using EventStore.Dispatcher;
	using Machine.Specifications;
	using Persistence.EventStore;

	[Subject("Persistence")]
	public class when_an_aggregate_is_persisted: in_the_event_store
	{
		static readonly Guid id = Guid.NewGuid();
		static TestAggregate aggregate;

		Establish context = () => aggregate = new TestAggregate(id, "Some name");

		Because of = () =>
			repository.Save(aggregate, Guid.NewGuid(), null);

		It should_be_returned_when_calling_get_by_id = () =>
			repository.GetById<TestAggregate>(id, 0).Id.ShouldEqual(aggregate.Id);
	}

	[Subject("Persistence")]
	public class when_an_aggregate_is_updated : in_the_event_store
	{
		static readonly Guid Id = Guid.NewGuid();
		const string NewName = "New name";

		Establish context = () =>
			repository.Save(new TestAggregate(Id, "Some name"), Guid.NewGuid(), null);

		Because of = () =>
		{
			var aggregate = repository.GetById<TestAggregate>(Id, 0);
			aggregate.ChangeName(NewName);

			repository.Save(aggregate, Guid.NewGuid(), null);
		};
		
		It should_the_version_number_should_increase = () =>
			repository.GetById<TestAggregate>(Id, 0).Version.ShouldEqual(2);

		It should_update_the_aggregate = () =>
			repository.GetById<TestAggregate>(Id, 0).Name.ShouldEqual(NewName);
	}

	[Subject("Persistence")]
	public class when_an_aggregate_is_updated_again_after_being_saved : in_the_event_store
	{
		static readonly Guid Id = Guid.NewGuid();
		const string NewName = "New name";
		const string SecondChange = "Second Name";

		Establish context = () =>
			repository.Save(new TestAggregate(Id, "Some name"), Guid.NewGuid(), null);

		Because of = () =>
		{
			var aggregate = repository.GetById<TestAggregate>(Id, 0);

			aggregate.ChangeName(NewName);
			repository.Save(aggregate, Guid.NewGuid(), null);

			aggregate.ChangeName(SecondChange);
			repository.Save(aggregate, Guid.NewGuid(), null);
		};

		It should_increase_the_version_number = () =>
			repository.GetById<TestAggregate>(Id, 0).Version.ShouldEqual(3);

		It should_update_the_aggregate = () =>
			repository.GetById<TestAggregate>(Id, 0).Name.ShouldEqual(SecondChange);
	}

	[Subject("Persistence")]
	public class when_an_aggregate_is_updated_by_two_different_repository_instances_using_the_same_store_and_the_second_update_has_less_events_than_the_first : in_the_event_store
	{
		static readonly Guid Id = Guid.NewGuid();
		const string NewName = "New name";
		const string NewerName = "Newer name";
		const string FinalName = "Final Name";

		static EventStoreRepository repository1;
		static EventStoreRepository repository2;
		private static ConflictDetector conflictDetector;

		Establish context = () =>
		{
			repository.Save(new TestAggregate(Id, "Some name"), Guid.NewGuid(), null); // 1
			conflictDetector = new ConflictDetector();
			conflictDetector.Register<NameChangedEvent, NameChangedEvent>((event1, event2) => false);
			repository1 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);
			repository2 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);
		};
		Because of = () =>
		{
			// simulate thread1
			var aggregate1 = repository1.GetById<TestAggregate>(Id, 0);
			aggregate1.ChangeName(NewName); // 2
			aggregate1.ChangeName(NewName); // 3
			aggregate1.ChangeName(NewName); // 4

			// simulate thread2
			var aggregate2 = repository2.GetById<TestAggregate>(Id, 0);
			aggregate2.ChangeName(NewerName); // 5
			aggregate2.ChangeName(FinalName); // 6

			// simulate thread1
			repository1.Save(aggregate1, Guid.NewGuid(), null);

			// simulate thread2 - this should throw a concurrency exception and force a single retry
			repository2.Save(aggregate2, Guid.NewGuid(), null);

			// re-open repository 1 to clear it's cache
			repository1.Dispose();
			repository1 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);
		};

		It should_increase_the_version_number_for_both_repositories = () =>
		{
			repository1.GetById<TestAggregate>(Id, 0).Version.ShouldEqual(6);
			repository2.GetById<TestAggregate>(Id, 0).Version.ShouldEqual(6);
		};

		It should_update_the_aggregate_with_the_last_commit = () =>
			repository1.GetById<TestAggregate>(Id, 0).Name.ShouldEqual(FinalName);
	}

	[Subject("Persistence")]
	public class when_an_aggregate_is_updated_by_two_different_repository_instances_using_the_same_store_and_the_second_update_has_more_events_than_the_entire_stream : in_the_event_store
	{
		static readonly Guid Id = Guid.NewGuid();
		const string NewName = "New name";
		const string NewerName = "Newer name";
		const string FinalName = "Final Name";

		static EventStoreRepository repository1;
		static EventStoreRepository repository2;
		static ConflictDetector conflictDetector;

		Establish context = () =>
		{
			repository.Save(new TestAggregate(Id, "Some name"), Guid.NewGuid(), null); // 1
			conflictDetector = new ConflictDetector();
			conflictDetector.Register<NameChangedEvent, NameChangedEvent>((event1, event2) => false);
			repository1 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);
			repository2 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);
		};
		Because of = () =>
		{
			// simulate thread1
			var aggregate1 = repository1.GetById<TestAggregate>(Id, 0);
			aggregate1.ChangeName(NewName); // 2
			aggregate1.ChangeName(NewName); // 3
			aggregate1.ChangeName(NewName); // 4

			// simulate thread2
			var aggregate2 = repository2.GetById<TestAggregate>(Id, 0);
			aggregate2.ChangeName(NewerName); // 5
			aggregate2.ChangeName(NewerName); // 6
			aggregate2.ChangeName(NewerName); // 7
			aggregate2.ChangeName(NewerName); // 8
			aggregate2.ChangeName(NewerName); // 9
			aggregate2.ChangeName(FinalName); // 10

			// simulate thread1
			repository1.Save(aggregate1, Guid.NewGuid(), null);

			// simulate thread2 - this should throw a concurrency exception and force a single retry
			repository2.Save(aggregate2, Guid.NewGuid(), null);

			// re-open repository 1 to clear it's cache
			repository1.Dispose();
			repository1 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);
		};

		It should_increase_the_version_number_for_both_repositories = () =>
		{
			repository1.GetById<TestAggregate>(Id, 0).Version.ShouldEqual(10);
			repository2.GetById<TestAggregate>(Id, 0).Version.ShouldEqual(10);
		};

		It should_update_the_aggregate_with_the_last_commit = () =>
			repository1.GetById<TestAggregate>(Id, 0).Name.ShouldEqual(FinalName);
	}

	/// <summary>
	/// This test would simulate cross process concurrency
	/// </summary>
	[Subject("Persistence")]
	public class when_an_aggregate_is_updated_concurrently_on_two_threads_using_different_stores : in_the_event_store
	{
		static readonly Guid Id = Guid.NewGuid();
		const string NewName = "New name";
		const string NewerName = "Newer name";

		static readonly IList<IDomainEvent> publishedEvents1 = new List<IDomainEvent>();
		static readonly IList<IDomainEvent> publishedEvents2 = new List<IDomainEvent>();

		static OptimisticEventStore eventStore1;
		static OptimisticEventStore eventStore2;

		static EventStoreRepository repository1;
		static EventStoreRepository repository2;

		Establish context = () =>
		{
			eventStore1 = new OptimisticEventStore(engine, new SynchronousDispatcher(new FakeBus(publishedEvents1), engine));
			eventStore2 = new OptimisticEventStore(engine, new SynchronousDispatcher(new FakeBus(publishedEvents2), engine));

            var conflictDetector = new ConflictDetector();
            conflictDetector.Register<NameChangedEvent, NameChangedEvent>((event1, event2) => false);
            repository1 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);
            repository2 = new EventStoreRepository(eventStore, new AggregateFactory(), conflictDetector);

			repository1.Save(new TestAggregate(Id, "Some name"), Guid.NewGuid(), null);
		};

		Because of = () =>
		{
			// simulate T1
			var aggregate = repository1.GetById<TestAggregate>(Id, 0);
			aggregate.ChangeName(NewName);

			// simulate T2
			var aggregate2 = repository2.GetById<TestAggregate>(Id, 0);
			aggregate2.ChangeName(NewerName);

			repository1.Save(aggregate, Guid.NewGuid(), null);
			repository2.Save(aggregate2, Guid.NewGuid(), null);
		};

		It the_version_number_should_increase_to_include_all_commits = () =>
			repository2.GetById<TestAggregate>(Id, 0).Version.ShouldEqual(3);

		It should_update_the_aggregate = () =>
			repository2.GetById<TestAggregate>(Id, 0).Name.ShouldEqual(NewerName);
	}

	public class TestAggregate : AggregateBase<IDomainEvent>
	{
		public string Name { get; set; }
		public TestAggregate(Guid id)
		{
			this.Register<TestAggregateCreatedEvent>(this.Apply);
			this.Register<NameChangedEvent>(this.Apply);
			this.Id = id;
		}

		public TestAggregate(Guid id, string name)
			:this(id)
		{
			this.RaiseEvent(new TestAggregateCreatedEvent
			{
				Id = this.Id,
				Name = name
			});
		}

		private void Apply(TestAggregateCreatedEvent @event)
		{
			this.Id = @event.Id;
			this.Name = @event.Name;
		}

		public void ChangeName(string newName)
		{
			this.RaiseEvent(new NameChangedEvent { Name = newName });
		}
		private void Apply(NameChangedEvent @event)
		{
			this.Name = @event.Name;
		}
	}

	[Serializable]
	public class NameChangedEvent : IDomainEvent
	{
		public string Name { get; set; }
	}

	[Serializable]
	public class TestAggregateCreatedEvent : IDomainEvent
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}

// ReSharper enable InconsistentNaming
#pragma warning restore 169