namespace CommonDomain
{
	using System;

	public interface IMomento
	{
		Guid Id { get; set; }
		long Version { get; set; }
	}
}