using LibraryModels;
using LibraryQuerying;



namespace LibraryRepositroy.Common
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync(PaginationQuery query);
        Task<Category?> GetByIdAsync(Guid Id);
        Task<Category?> CreateAsync(Category category);
        Task<Category?> UpdateAsync(Category category);
        Task<bool> DeleteAsync(Guid id);
        Task<int> GetTotalCountAsync(PaginationQuery query);

        Task<List<CategoryWithBooks>> GetCategoriesWithBooksAsync();
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name);
        Task<HashSet<Guid>> GetExistingCategoryIdsAsync(List<Guid> ids);
        Task<List<Category>> GetAllCategoriesAsync();


    }
}
