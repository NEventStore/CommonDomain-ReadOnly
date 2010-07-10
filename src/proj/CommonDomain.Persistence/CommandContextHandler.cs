namespace CommonDomain.Persistence
{
	using System;

	public class CommandContextHandler : IHandleCommandContext
	{
		private CommandContext current;

		public void SetCurrent(Guid id, long version, object message)
		{
			this.current = new CommandContext(id, version, message);
		}

		public CommandContext GetCurrent()
		{
			return this.current;
		}
	}
}