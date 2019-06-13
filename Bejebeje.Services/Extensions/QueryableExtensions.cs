namespace Bejebeje.Services.Extensions
{
  using System.Linq;

  public static class QueryableExtensions
  {
    public static IQueryable<T> Paging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
            where T : class
    {
      IQueryable<T> paged = query
              .Skip((pageNumber - 1) * pageSize)
              .Take(pageSize);

      return paged;
    }
  }
}
