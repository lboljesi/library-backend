using LibraryModels;
using LibraryQuerying;
using LibraryRepository;
using LibraryRepository.Common;
using LibraryRepositroy.Common;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc.Formatters;
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
        private readonly IAuthorRepository _authorRepository;
        private readonly ICategoryRepository _categoryRepository;
        public BookService(IBookRepository repository, IAuthorRepository authorRepository, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _authorRepository = authorRepository;
            _categoryRepository = categoryRepository;
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

        public async Task<Guid> AddBookAsync(BooksCreateUpdate dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required");
            if (string.IsNullOrWhiteSpace(dto.Isbn))
                throw new ArgumentException("ISBN is required");
            if (dto.PublishedYear < 0 || dto.PublishedYear > DateTime.Now.Year)
                throw new ArgumentException("Invalid published year");
            if (dto.Price < 0)
                throw new ArgumentException("Invalid price");

            if (dto.AuthorIds != null && dto.AuthorIds.Any())
            {
                var validAuthors = await _authorRepository.GetExistingAuthorIdsAsync(dto.AuthorIds);
                if (validAuthors.Count != dto.AuthorIds.Count)
                    throw new InvalidOperationException("One or more authors do not exist");
            }

            if (dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                var validCategories = await _categoryRepository.GetExistingCategoryIdsAsync(dto.CategoryIds);
                if (validCategories.Count != dto.CategoryIds.Count)
                    throw new InvalidOperationException("One or more categories do not exist");
            }

            return await _repository.AddBookAsync(dto);
        }

        public async Task<BookDto?> GetBookByIdAsync(Guid id)
        {
            return await _repository.GetBookByIdAsync(id);
        }

        public async Task<bool> DeleteBookAsync(Guid id)
        {
            return await _repository.DeleteBookAsync(id);
        }

        public async Task<bool> DeleteBookBulkAsync(List<Guid> ids)
        {

            return await _repository.DeleteBookBulkAsync(ids);
        }
    }
}
