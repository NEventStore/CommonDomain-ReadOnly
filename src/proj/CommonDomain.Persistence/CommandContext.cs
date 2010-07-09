namespace CommonDomain.Persistence
{
	using System;

	public class CommandContext
	{
		public CommandContext(Guid id, long version, object message)
		{
			this.Id = id;
			this.Version = version;
			this.Message = message;
		}

		public Guid Id { get; private set; }
		public long Version { get; private set; }
		public object Message { get; private set; }
	}
}