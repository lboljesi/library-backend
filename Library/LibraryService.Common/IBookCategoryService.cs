using LibraryModels;
using LibraryQuerying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.Common
{
    public interface IBookCategoryService
    {
        Task<bool> DeleteBookCategoryAsync(Guid id);
        Task<BookCategory?> CreateBookCategoryAsync(CreateBookCategoryDto dto);
        Task<BookCategory?> GetByIdAsync(Guid id);
        Task<List<BookCategoryJOIN>> GetAllAsync(SortablePaginationQuery query);
        Task<List<CategoryWithRelation>> CreateManyAsync(Guid bookid, List<Guid> categoryIds);
        Task<List<Books>> GetBooksForCategoryAsync(Guid categoryId);
        Task<PagedResult<BookWithCategoriesDto>> GetGroupedByBooksAsync(PaginationQuery query);

        Task<int> CountGroupedBookAsync(PaginationQuery query);
        Task<List<Category>> GetCategoriesWithoutBooksAsync();
        Task<int> DeleteRelationsAsync(List<Guid> relationIds);
        Task<List<BookWithAuthorsDto>> GetBooksWithAuthorsForCategoryAsync(Guid categoryId);
    }
}
