#pragma warning disable 169
// ReSharper disable InconsistentNaming

namespace CommonDomain.AcceptanceTests
{
	using System;
	using Core;
	using Machine.Specifications;

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