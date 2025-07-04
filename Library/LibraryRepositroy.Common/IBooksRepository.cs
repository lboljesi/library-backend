using LibraryModels;
using LibraryQuerying;

namespace LibraryRepository
{
    public interface IBooksRepository
    {
        Task<Books> CreateBookAsync(Books book);
        Task<bool> DeleteBook(Guid Id);
        Task<Books> GetBookByIdAsync(Guid Id);
        Task<List<Books>> GetBooksAsync(BooksQuery query);
        Task<Books> UpdateBookAsync(Books book);
        Task<bool> ExistsAsync(Guid id);
    }
}