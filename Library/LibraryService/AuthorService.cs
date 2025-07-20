using LibraryModels;
using LibraryRepository.Common;
using LibraryService.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _repository;
        public AuthorService(IAuthorRepository repository)
        {
            _repository = repository;
        }
        public async Task<List<AuthorDto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
