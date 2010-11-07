namespace CommonDomain.Persistence
{
	using System.Collections;

	public interface IStampAggregateVersion
	{
		void SetVersion(ICollection events, long startingVersion);
	}
}