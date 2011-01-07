namespace CommonDomain
{
	using System;

	public interface IRouteEvents<TEvent>
	{
		void Register<TEventMessage>(Action<TEventMessage> handler) where TEventMessage : TEvent;
		void Register(IAggregate aggregate);

		void Dispatch(object eventMessage);
	}
}