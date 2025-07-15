using LibraryRestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.Common
{
    public interface IUserService
    {
        Task<string?> ValidateLoginAndGenerateTokenAsync(string email, string password);
        Task<string?> RegisterUserAsync(string email, string password, string fullName);
    }
}
