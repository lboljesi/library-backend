using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryModels;
using LibraryRepository;

namespace LibraryService
{
    public class BookAuthorsService : IBookAuthorsRepository, IBookAuthorsService
    {
        private readonly IBookAuthorsRepository _bookauthorsRepository;

        public BookAuthorsService(IBookAuthorsRepository bookauthorsRepository)
        {
            _bookauthorsRepository = bookauthorsRepository;
        }

        public async Task<List<BookAuthors>> GetBookAuthorsAsync()
        {
            var bookAuthors = await _bookauthorsRepository.GetBookAuthorsAsync();
            return bookAuthors;

        }
        public async Task<BookAuthors> GetBookAuthorsByIdAsync(Guid id)
        {

            var bookAuthor = await _bookauthorsRepository.GetBookAuthorsByIdAsync(id);
            return bookAuthor;
        }

        public async Task<BookAuthors> UpdateBookAuthorAsync(BookAuthors bookAuthor)
        {
            var updatedBookAuthor = await _bookauthorsRepository.UpdateBookAuthorAsync(bookAuthor);
            return updatedBookAuthor;

        }
        public async Task<BookAuthors> CreateBookAuthorsAsync(BookAuthors bookAuthors)
        {

            
            var newbookAuthor = await _bookauthorsRepository.CreateBookAuthorsAsync(bookAuthors);
            return newbookAuthor;

        }
        public async Task<bool> DeleteBookAuthorsAsync(Guid Id)
        {
            bool deleted = await _bookauthorsRepository.DeleteBookAuthorsAsync(Id);

            if (deleted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<Books>> GetAllBooksByAuthorAsync(Guid id)
        {

            var booksbyAuthor = await _bookauthorsRepository.GetAllBooksByAuthorAsync(id);
            return booksbyAuthor;
        }

        public async Task<List<Authors>> GetAllAuthorsByBookAsync(Guid id)
        {

            var authorsbyBook = await _bookauthorsRepository.GetAllAuthorsByBookAsync(id);
            return authorsbyBook;
        }

    }
}
