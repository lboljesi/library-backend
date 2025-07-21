using LibraryModels;
using LibraryQuerying;

namespace LibraryRepository
{
    public interface IBookRepository
    {
        Task<List<BookDto>> GetAllBooksAsync(SortablePaginationQuery query);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountBooksAsync(SortablePaginationQuery query);
        Task<Guid> AddBookAsync(BooksCreateUpdate dto);
        Task<BookDto?> GetBookByIdAsync(Guid bookId);
        Task<bool> DeleteBookAsync(Guid id);
        Task<bool> DeleteBookBulkAsync(List<Guid> ids);
    }
}