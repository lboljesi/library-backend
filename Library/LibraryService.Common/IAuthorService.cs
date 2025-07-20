using LibraryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.Common
{
    public interface IAuthorService
    {
        Task<List<AuthorDto>> GetAllAsync();
    }
}
