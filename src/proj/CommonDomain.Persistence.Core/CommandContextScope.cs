namespace CommonDomain.Persistence.Core
{
	using System;

	public class CommandContextScope : IScopeCommandContext
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