using LibraryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository.Common
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task CreateUserAsync(string email, string passwordHash, string fullName);
    }
}
