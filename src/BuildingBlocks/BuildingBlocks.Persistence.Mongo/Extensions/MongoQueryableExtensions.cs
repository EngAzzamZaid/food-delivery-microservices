using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Core.Linq;
using BuildingBlocks.Core.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace BuildingBlocks.Persistence.Mongo.Extensions;

public static class MongoQueryableExtensions
{
    public static async Task<ListResultModel<T>> ApplyPagingAsync<T>(
        this IMongoQueryable<T> collection,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
        where T : notnull
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty)
            return ListResultModel<T>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = await collection.Skip(page, pageSize).ToListAsync(cancellationToken: cancellationToken);

        return ListResultModel<T>.Create(data, totalItems, page, pageSize);
    }

    public static async Task<ListResultModel<TR>> ApplyPagingAsync<T, TR>(
        this IMongoQueryable<T> collection,
        IConfigurationProvider configuration,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    )
        where TR : notnull
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        var isEmpty = await collection.AnyAsync(cancellationToken: cancellationToken) == false;
        if (isEmpty)
            return ListResultModel<TR>.Empty;

        var totalItems = await collection.CountAsync(cancellationToken: cancellationToken);
        var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
        var data = collection.Skip(page, pageSize).ProjectTo<TR>(configuration).ToList();

        return ListResultModel<TR>.Create(data, totalItems, page, pageSize);
    }

    public static IMongoQueryable<T> Skip<T>(this IMongoQueryable<T> collection, int page = 1, int resultsPerPage = 10)
    {
        if (page <= 0)
            page = 1;

        if (resultsPerPage <= 0)
            resultsPerPage = 10;

        var skip = (page - 1) * resultsPerPage;
        var data = MongoQueryable.Skip(collection, skip).Take(resultsPerPage);

        return data;
    }

    public static IMongoQueryable<TEntity> ApplyFilter<TEntity>(
        this IMongoQueryable<TEntity> source,
        IEnumerable<FilterModel>? filters
    )
        where TEntity : class
    {
        if (filters is null)
            return source;

        List<Expression<Func<TEntity, bool>>> filterExpressions = new List<Expression<Func<TEntity, bool>>>();

        foreach (var (fieldName, comparison, fieldValue) in filters)
        {
            Expression<Func<TEntity, bool>> expr = PredicateBuilder.Build<TEntity>(fieldName, comparison, fieldValue);
            filterExpressions.Add(expr);
        }

        return source.Where(filterExpressions.Aggregate((expr1, expr2) => expr1.And(expr2)));
    }

    public static Expression<Func<TEntity, bool>> ApplyAndFilter<TEntity>(
        this IList<Expression<Func<TEntity, bool>>> filterExpressions
    )
    {
        return filterExpressions.Aggregate((expr1, expr2) => expr1.And(expr2));
    }

    public static IDictionary<string, Expression<Func<TEntity, bool>>> ConvertToEntityFilters<TEntity>(
        this IEnumerable<FilterModel>? filters
    )
        where TEntity : class
    {
        Dictionary<string, Expression<Func<TEntity, bool>>> filterExpressions = new Dictionary<
            string,
            Expression<Func<TEntity, bool>>
        >(StringComparer.OrdinalIgnoreCase);
        if (filters is null)
            return filterExpressions;

        foreach (var (fieldName, comparison, fieldValue) in filters)
        {
            Expression<Func<TEntity, bool>> expr = PredicateBuilder.Build<TEntity>(fieldName, comparison, fieldValue);
            filterExpressions.Add(fieldName, expr);
        }

        return filterExpressions;
    }
}
