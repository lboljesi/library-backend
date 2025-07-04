using LibraryModels;
using LibraryQuerying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.Common
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync(PaginationQuery query);
        Task<Category?> GetCategoryByIdAsync(Guid id);
        Task<Category?> CreateCategoryAsync(Category category);
        Task<Category?> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<int> GetTotalCountAsync(PaginationQuery query);
        Task<List<CategoryWithBooks>> GetCategoriesWithBooksAsync();

        Task<PagedResult<Category>> GetPagedCategoriesAsync(PaginationQuery query);
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
