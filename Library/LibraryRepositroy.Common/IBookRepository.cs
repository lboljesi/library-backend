using LibraryModels;
using LibraryQuerying;

namespace LibraryRepository
{
    public interface IBookRepository
    {
        Task<List<BookDto>> GetAllBooksAsync(BookQuery query);
        Task<bool> ExistsAsync(Guid id);
        Task<int> CountBooksAsync(BookQuery query);
        Task<Guid> AddBookAsync(BooksCreateUpdate dto);
        Task<BookDto?> GetBookByIdAsync(Guid bookId);
        Task<bool> DeleteBookAsync(Guid id);
        Task<bool> DeleteBookBulkAsync(List<Guid> ids);
        Task<bool> UpdateBookAsync(Guid id, BookUpdateDto dto);
        Task<List<AuthorDto>> GetAuthorsByBookIdAsync(Guid id);
        Task<List<Category>> GetCategoriesByBookIdAsync(Guid id);
    }
}