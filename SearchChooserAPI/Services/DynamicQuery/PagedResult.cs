namespace Meccano.DynamicQuery;

public class PagedResult<T>
    {
    public List<T>   Items { get; set; } = [];

    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 1;
    public bool HasNext => PageNumber > 0 && PageNumber < TotalPages;

    public bool HasPrevious => PageNumber > 1;
    
    }

