using LibraryModels;
using LibraryQuerying;

namespace LibraryRepository
{
    public interface IBookRepository
    {
        Task<List<BookDto>> GetAllBooksAsync(SortablePaginationQuery query);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountBooksAsync(SortablePaginationQuery query);
    }
}