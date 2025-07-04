using LibraryModels;
using LibraryQuerying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository.Common
{
    public interface IBookCategoryRepository
    {
        Task<bool> DeleteAsync(Guid id);
        Task<BookCategory?> CreateAsync(CreateBookCategoryDto dto);
        Task<BookCategory?> GetByIdAsync(Guid id);
        Task<List<BookCategoryJOIN>> GetAllAsync(BookCategoryQuery query);
        Task<bool> ExistsAsync(Guid bookId, Guid categoryId);
        Task<List<CategoryWithRelation>> CreateManyAsync(Guid bookId, List<Guid> categoryIds);
        Task<List<Books>> GetBooksForCategoryAsync(Guid categoryId);
        Task<PagedResult<BookWithCategoriesDto>> GetGroupedByBookAsync(PaginationQuery query);
        Task<List<Category>> GetCategoriesWithoutBooksAsync();

        Task<int> CountGroupedBookAsync(PaginationQuery query);
    }
}
