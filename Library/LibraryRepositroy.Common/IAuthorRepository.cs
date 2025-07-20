using LibraryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository.Common
{
    public interface IAuthorRepository
    {  

        Task<HashSet<Guid>> GetExistingAuthorIdsAsync(List<Guid> ids);
        Task<List<AuthorDto>> GetAllAsync();
    }
}
