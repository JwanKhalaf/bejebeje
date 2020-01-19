namespace Bejebeje.Services.Extensions
{
  using System.Linq;

  public static class QueryableExtensions
  {
    public static IQueryable<T> Paging<T>(this IQueryable<T> query, int offset, int limit)
            where T : class
    {
      IQueryable<T> paged = query
              .Skip(offset)
              .Take(limit);

      return paged;
    }
  }
}
