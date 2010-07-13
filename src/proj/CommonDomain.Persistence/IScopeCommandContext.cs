namespace CommonDomain.Persistence
{
	using System;

	public interface IScopeCommandContext
	{
		void SetCurrent(Guid id, long version, object message);
		CommandContext GetCurrent();
	}
}