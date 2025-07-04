using LibraryModels;
using LibraryQuerying;

namespace LibraryService
{
    public interface IBooksService
    {
        Task<Books> CreateBookAsync(Books book);
        Task<bool> DeleteBook(Guid Id);
        Task<Books> GetBookByIdAsync(Guid Id);
        Task<List<Books>> GetBooksAsync(BooksQuery query);
        Task<Books> UpdateBookAsync(Books book);
    }
}