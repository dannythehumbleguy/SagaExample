using MongoDB.Driver;

namespace PaymentService.Api.Common.Pagination;

public static class MongoDbPaginationExtensions
{
    public static IFindFluent<TDocument, TDocument> ApplyPaging<TDocument>(
        this IFindFluent<TDocument, TDocument> query, 
        PaginationRequest request)
    {
        return query.Skip(request.Skip()).Limit(request.PageSize);
    }
 
    public static async Task<Paged<TDocument>> ToPagedResultAsync<TDocument>(
        this IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCountTask = collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = collection.Find(filter)
            .Skip(request.Skip())
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);
            
        await Task.WhenAll(totalCountTask, itemsTask);
            
        return new Paged<TDocument>(
            itemsTask.Result, 
            totalCountTask.Result, 
            request.PageNumber, 
            request.PageSize);
    }
    
    public static async Task<Paged<TDocument>> ToPagedResultAsync<TDocument>(
        this IMongoCollection<TDocument> collection,
        FilterDefinition<TDocument> filter,
        SortDefinition<TDocument> sort,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCountTask = collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var itemsTask = collection.Find(filter)
            .Sort(sort)
            .Skip(request.Skip())
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);
            
        await Task.WhenAll(totalCountTask, itemsTask);
            
        return new Paged<TDocument>(
            itemsTask.Result, 
            totalCountTask.Result, 
            request.PageNumber, 
            request.PageSize);
    }
}