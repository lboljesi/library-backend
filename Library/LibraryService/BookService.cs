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

        public async Task<BooksPagedDto> GetBooksWithPaginationAsync(BookQuery query)
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

        public async Task<bool> UpdateBookAsync(Guid id, BookUpdateDto dto)
        {
            return await _repository.UpdateBookAsync(id, dto);
        }

        public async Task<List<AuthorWithLinkDto>> GetAuthorsByBookIdAsync(Guid id)
        {
            return await _repository.GetAuthorsByBookIdAsync(id);
        }

        public async Task<List<CategoryWithLinkDto>> GetCategoriesByBookIdAsync(Guid id)
        {
            return await _repository.GetCategoriesByBookIdAsync(id);
        }
        public async Task<bool> DeleteBookAuthorAsync(Guid bookAuthorId)
        {
            return await _repository.DeleteBookAuthorAsync(bookAuthorId);
        }
        public async Task<bool> DeleteBookCategoryAsync(Guid id)
        {
            return await _repository.DeleteBookCategoryAsync(id);
        }
        public async Task<List<AuthorWithLinkDto>> AddAuthorsToBookAsync(Guid bookId, List<Guid> authorIds)
        {
            if (authorIds == null || !authorIds.Any())
                throw new ArgumentException("At least one author must be provided!");

            if (!await _repository.ExistsAsync(bookId))
                throw new InvalidOperationException("Book does not exist");

            var validAuthorIds = await _authorRepository.GetExistingAuthorIdsAsync(authorIds);

            var filtered = new List<Guid>();
            foreach (var authorId in validAuthorIds)
            {
                if(!await _repository.ExistsRelationAsync(bookId, authorId))
                {
                    filtered.Add(authorId);
                }
            }

            if(!filtered.Any())
            {
                throw new InvalidOperationException("All realtions already exist or are invalid");
            }

            var result = await _repository.AddAuthorsToBookAsync(bookId, authorIds);
            if (!result.Any())
                throw new InvalidOperationException("No relations were created");

            return result;
        }
    }
}
