using System;

namespace CommonDomain.Persistence
{
  public static class RepositoryExtensions
  {
    public static TAggregate GetById<TAggregate>(this IRepository repository, Guid id) where TAggregate : class, IAggregate
    {
      return repository.GetById(typeof(TAggregate), id, int.MaxValue) as TAggregate;
    }

    public static TAggregate GetById<TAggregate>(this IRepository repository, Guid id, int versionToLoad) where TAggregate : class, IAggregate
    {
      return repository.GetById(typeof(TAggregate), id, versionToLoad) as TAggregate;
    }

    public static void Save(this IRepository repository, IAggregate aggregate, Guid commitId)
    {
      repository.Save(aggregate, commitId, a => { });
    }
  }
}