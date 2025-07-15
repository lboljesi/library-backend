using LibraryModels;
using LibraryQuerying;

namespace LibraryRepository
{
    public interface IBooksRepository
    {
        Task<bool> ExistsAsync(Guid id);
    }
}