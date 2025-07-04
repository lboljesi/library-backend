using LibraryModels;
using LibraryQuerying;
using LibraryRepositroy.Common;
using LibraryService.Common;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LibraryService
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ILogger<CategoryService> _logger;
        public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public Task<List<Category>> GetAllCategoriesAsync(PaginationQuery query)
        {
            return _repository.GetAllAsync(query);
        }

        public async Task<PagedResult<Category>> GetPagedCategoriesAsync(PaginationQuery query)
        {
            var categories = await _repository.GetAllAsync(query);
            var total = await _repository.GetTotalCountAsync(query);

            return new PagedResult<Category>
            {
                Items = categories,
                TotalCount = total,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        public Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            return _repository.GetByIdAsync(id);
        }

        public async Task<Category?> CreateCategoryAsync(Category category)
        {
            var exists = await _repository.ExistsByNameAsync(category.Name);
            if(exists)
            {
                _logger.LogWarning($"Attempt to create duplicate category {category.Name}");
                throw new InvalidOperationException("A category with this name already exists.");
            }
            return await _repository.CreateAsync(category);
        }
        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            return await _repository.UpdateAsync(category);
        }
        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if(deleted)
            {
                _logger.LogInformation($"Category with Id {id} sucessfully deleted.");
            }
            else
            {
                _logger.LogWarning($"Attempt to delete non-existent category with Id: {id}");
            }
            return deleted;
        }

        public async Task<int> GetTotalCountAsync(PaginationQuery query)
        {
            return await _repository.GetTotalCountAsync(query);
        }

        public async Task<List<CategoryWithBooks>> GetCategoriesWithBooksAsync()
        {
            return await _repository.GetCategoriesWithBooksAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllCategoriesAsync();
        }

    }
}
