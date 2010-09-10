namespace CommonDomain
{
	using System;

	public interface IMemento
	{
		Guid Id { get; set; }
		long Version { get; set; }
	}
}