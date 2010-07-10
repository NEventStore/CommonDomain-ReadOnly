namespace CommonDomain.Persistence
{
	using System;

	public class CommandContext
	{
		public CommandContext(Guid id, long version, object message)
		{
			this.Id = id;
			this.Version = version >= 0 ? version : 0;
			this.Message = message;
		}

		public Guid Id { get; private set; }
		public long Version { get; private set; }
		public object Message { get; private set; }
	}
}