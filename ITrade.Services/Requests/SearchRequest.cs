namespace ITrade.Services.Requests
{
    public record SearchRequest
    (
        string? Query = null,
        SearchEntityType? EntityType = null,
        ICollection<int>? TagIds = null,
        int? UserRoleId = null,
        int? ProjectStatusId = null,
        DateTime? DeadlineFrom = null,
        DateTime? DeadlineTo = null,
        DateTime? CreatedFrom = null,
        DateTime? CreatedTo = null,
        SearchSortBy SortBy = SearchSortBy.Relevance,
        SortDirection SortDirection = SortDirection.Descending,
        int Page = 1,
        int PageSize = 20
    );

    public enum SearchEntityType
    {
        Projects = 1,
        Users = 2,
        All = 3
    }

    public enum SearchSortBy
    {
        Relevance = 1,
        CreatedAt = 2,
        Name = 3,
        Deadline = 4
    }

    public enum SortDirection
    {
        Ascending = 1,
        Descending = 2
    }
}
