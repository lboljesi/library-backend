using LibraryModels;
using LibraryQuerying;
using LibraryRepository;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryService
{
    public class BooksService : IBooksService
    {
        private readonly IBooksRepository _booksRepository;

        public BooksService(IBooksRepository booksRepository)
        {
            _booksRepository = booksRepository;
        }

        public async Task<List<Books>> GetBooksAsync(BooksQuery query)
        {
            var books = await _booksRepository.GetBooksAsync(query);
            return books;

        }
        public async Task<Books> GetBookByIdAsync(Guid Id)
        {
            var book = await _booksRepository.GetBookByIdAsync(Id);
            return book;

        }
        public async Task<Books> UpdateBookAsync(Books book)
        {
            var updatedBook = await _booksRepository.UpdateBookAsync(book);
            return updatedBook;

        }
        public async Task<Books> CreateBookAsync(Books book)
        {
            var query = new BooksQuery();
            var allBooks = await _booksRepository.GetBooksAsync(query);
            {
                if (allBooks.Any(a => a.Title == book.Title))
                {
                    Console.WriteLine($"{book.Title} already exists");
                    throw new InvalidOperationException("Book already exists");

                }
            }
            var newBook = await _booksRepository.CreateBookAsync(book);
            return newBook;

        }
        public async Task<bool> DeleteBook(Guid Id)
        {
            bool delete = await _booksRepository.DeleteBook(Id);
            return delete;


        }

    }
}
