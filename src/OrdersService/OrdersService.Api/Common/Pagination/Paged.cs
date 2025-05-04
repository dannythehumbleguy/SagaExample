namespace OrdersService.Api.Common.Pagination;


public class Paged<T>
{
    public List<T> Items { get; set; }
    
    public long TotalCount { get; set; }
    
    public int PageNumber { get; set; }
    
    public int PageSize { get; set; }
    
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => PageNumber < TotalPages;
    
    public Paged(List<T> items, long totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
    
    public Paged()
    {
        Items = [];
        TotalCount = 0;
        PageNumber = 1;
        PageSize = 10;
    }
}

public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;
    
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
    }
    
    public int Skip() => (PageNumber - 1) * PageSize;
}