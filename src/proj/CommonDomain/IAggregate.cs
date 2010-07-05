namespace CommonDomain
{
	using System;

	public interface IAggregate : IApplyEvents, IProduceEvents
	{
		Guid Id { get; }
	}
}