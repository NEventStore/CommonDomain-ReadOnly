namespace CommonDomain
{
	using System;
	using System.Collections;

	public interface IAggregate
	{
		Guid Id { get; }
		long Version { get; }

		void ApplyEvent(object @event);
		ICollection GetUncommittedEvents();
		void ClearUncommittedEvents();

		IMemento GetSnapshot();
	}
}