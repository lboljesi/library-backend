using LibraryModels;
using LibraryQuerying;

namespace LibraryService
{
    public interface IAuthorsService
    {
        Task<Authors> CreateAuthorAsync(Authors author);
        Task<bool> DeleteAuthor(Guid Id);
        Task<Authors> GetAuthorByIdAsync(Guid Id);
        
        Task<Authors> UpdateAuthorAsync(Authors author);

        Task<List<Authors>> GetAuthorsAsync(AuthorsQuery query);
    }
}