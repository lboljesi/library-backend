namespace DTOs
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public int PageSize {  get; set; }

    }
}
