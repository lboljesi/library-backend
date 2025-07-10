using LibraryModels;
using LibraryQuerying;
using LibraryRepository;
using LibraryRepository.Common;
using LibraryRepositroy.Common;
using LibraryService.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService
{
    public class BookCategoryService : IBookCategoryService
    {
        private readonly IBookCategoryRepository _repository;
        private readonly IBooksRepository _booksRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<BookCategoryService> _logger;

        public BookCategoryService(IBookCategoryRepository repository, IBooksRepository booksRepository, ICategoryRepository categoryRepository, ILogger<BookCategoryService> logger)
        {
            _repository = repository;
            _booksRepository = booksRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }
        public async Task<bool> DeleteBookCategoryAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }
        public async Task<BookCategory?> CreateBookCategoryAsync(CreateBookCategoryDto dto)
        {
            var bookExists = await _booksRepository.ExistsAsync(dto.BookId);
            var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId);
            if (!bookExists || !categoryExists)
            {
                _logger.LogWarning($"Invalid BookCategory creation: BookId {dto.BookId} or CategoryId {dto.CategoryId} does not exist.");
                return null;
            }
            var alreadyExists = await _repository.ExistsAsync(dto.BookId, dto.CategoryId);
            if (alreadyExists)
            {
                _logger.LogWarning($"Duplicate BookCategory relation attempted: BookId {dto.BookId}, CategoryId {dto.CategoryId}");
                return null;
            }
            var created = await _repository.CreateAsync(dto);
            _logger.LogInformation($"Created BookCategory relation BookId: {dto.BookId} ↔ CategoryId: {dto.CategoryId}");
            return created;
        }

        public async Task<BookCategory?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<BookCategoryJOIN>> GetAllAsync(BookCategoryQuery query)
        {
            return await _repository.GetAllAsync(query);
        }

        public async Task<List<Books>> GetBooksForCategoryAsync(Guid categoryId)
        {
            return await _repository.GetBooksForCategoryAsync(categoryId);
        }

        public async Task<List<CategoryWithRelation>> CreateManyAsync(Guid bookId, List<Guid> categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any())
                throw new ArgumentException("At least one category must be provided.");

            if (!await _booksRepository.ExistsAsync(bookId))
                throw new InvalidOperationException("Book does not exist.");

            var validCategoryIds = await _categoryRepository.GetExistingCategoryIdsAsync(categoryIds);

            // Fix for CS4010: Use Select with await inside and then filter using ToListAsync
            var filtered = new List<Guid>();
            foreach (var categoryId in validCategoryIds)
            {
                if (!await _repository.ExistsAsync(bookId, categoryId))
                {
                    filtered.Add(categoryId);
                }
            }

            if (!filtered.Any())
                throw new InvalidOperationException("All relations already exist or are invalid.");

            var result = await _repository.CreateManyAsync(bookId, filtered);

            if (!result.Any())
                throw new InvalidOperationException("No relations were created.");

            return result;
        }

        public async Task<PagedResult<BookWithCategoriesDto>> GetGroupedByBooksAsync(PaginationQuery query)
        {
            return await _repository.GetGroupedByBookAsync(query);
        }


        public async Task<int> CountGroupedBookAsync(PaginationQuery query)
        {
            return await _repository.CountGroupedBookAsync(query);
        }

        

        public async Task<List<Category>> GetCategoriesWithoutBooksAsync()
        {
            return await _repository.GetCategoriesWithoutBooksAsync();
        }

        public async Task<int> DeleteRelationsAsync(List<Guid> relationIds)
        {
            if (relationIds == null || relationIds.Count == 0)
                return 0;
            return await _repository.DeleteRelationsAsync(relationIds);
        }

        public async Task<List<BookWithAuthorsDto>> GetBooksWithAuthorsForCategoryAsync(Guid categoryId)
        {
            return await _repository.GetBooksWithAuthorsForCategoryAsync(categoryId);
        }


    }
}
