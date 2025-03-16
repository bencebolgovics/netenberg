using Netenberg.Model.Enums;
using System.Linq.Expressions;

namespace Netenberg.Database.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> OrderByField<T>(this IQueryable<T> query, string sortingField, SortingOrder sortingOrder)
    {
        if (string.IsNullOrWhiteSpace(sortingField))
            return query;

        var param = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(param, sortingField);
        var lambda = Expression.Lambda(property, param);

        string methodName = sortingOrder == SortingOrder.Descending ? "OrderByDescending" : "OrderBy";

        var result = typeof(Queryable).GetMethods()
            .First(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(T), property.Type)
            .Invoke(null, [query, lambda]);

        return (IQueryable<T>)result!;
    }
}
