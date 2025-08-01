using LibraryModels;
using LibraryQuerying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.Common
{
    public interface IBookService
    {
        Task<BooksPagedDto> GetBooksWithPaginationAsync(BookQuery query);

        Task<Guid> AddBookAsync(BooksCreateUpdate dto);
        Task<BookDto?> GetBookByIdAsync(Guid bookId);

        Task<bool> DeleteBookAsync(Guid id);
        Task<bool> DeleteBookBulkAsync(List<Guid> ids);

        Task<bool> UpdateBookAsync(Guid id, BookUpdateDto dto);
        Task<List<AuthorWithLinkDto>> GetAuthorsByBookIdAsync(Guid id);

        Task<List<CategoryWithLinkDto>> GetCategoriesByBookIdAsync(Guid id);
        Task<bool> DeleteBookAuthorAsync(Guid bookAuthorId);
        Task<bool> DeleteBookCategoryAsync(Guid id);
        Task<List<AuthorWithLinkDto>> AddAuthorsToBookAsync(Guid bookId, List<Guid> authorIds);
    }
}
