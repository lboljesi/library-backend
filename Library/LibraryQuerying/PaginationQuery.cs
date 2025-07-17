namespace LibraryQuerying
{
    public class PaginationQuery
    {
        public string? Search { get; set; }
        public bool Desc { get; set; } = false;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
