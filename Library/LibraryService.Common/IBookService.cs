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
        Task<BooksPagedDto> GetBooksWithPaginationAsync(SortablePaginationQuery query);

        Task<Guid> AddBookAsync(BooksCreateUpdate dto);
        Task<BookDto?> GetBookByIdAsync(Guid bookId);
    }
}
