using LibraryModels;
using LibraryQuerying;
using LibraryRepository;
using LibraryService.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;
        public BookService(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<BooksPagedDto> GetBooksWithPaginationAsync(SortablePaginationQuery query)
        {
            var totalCount = await _repository.CountBooksAsync(query);
            var books = await _repository.GetAllBooksAsync(query);

            return new BooksPagedDto { 
                TotalCount = totalCount,
                Books =books
            };
        }
    }
}
