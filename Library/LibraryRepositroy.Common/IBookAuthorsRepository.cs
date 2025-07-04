using LibraryModels;

namespace LibraryRepository
{
    public interface IBookAuthorsRepository
    {
        Task<BookAuthors> CreateBookAuthorsAsync(BookAuthors bookAuthors);
        Task<bool> DeleteBookAuthorsAsync(Guid Id);
        Task<List<BookAuthors>> GetBookAuthorsAsync();
        Task<BookAuthors> GetBookAuthorsByIdAsync(Guid id);
        Task<BookAuthors> UpdateBookAuthorAsync(BookAuthors bookAuthor);

        Task<List<Books>> GetAllBooksByAuthorAsync(Guid id);

        Task<List<Authors>> GetAllAuthorsByBookAsync(Guid id);
    }
}