namespace CommonDomain.Persistence
{
	using System;

	public interface IHandleCommandContext
	{
		void SetCurrent(Guid id, long version, object message);
		CommandContext GetCurrent();
	}
}