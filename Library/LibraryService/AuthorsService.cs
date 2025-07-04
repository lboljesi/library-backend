using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryModels;
using LibraryQuerying;
using LibraryRepository;


namespace LibraryService
{
    public class AuthorsService : IAuthorsService
    {
        public readonly IAuthorsRepository _authorsRepository;

        public AuthorsService(IAuthorsRepository authorsRepository)
        {
            _authorsRepository = authorsRepository;
        }


      
        public async Task<Authors> GetAuthorByIdAsync(Guid Id)
        {
            var author = await _authorsRepository.GetAuthorByIdAsync(Id);
            return author;

        }

        public async Task<Authors> UpdateAuthorAsync(Authors author)
        {
            var updatedAuthor = await _authorsRepository.UpdateAuthorAsync(author);
            return updatedAuthor;


        }

        public async Task<Authors> CreateAuthorAsync(Authors author)
        {
            var query = new AuthorsQuery();
            var allAuthors = await _authorsRepository.GetAuthorsAsync(query);
            {
                if(allAuthors.Any(a=>a.FirstName== author.FirstName && a.LastName == author.LastName))
                {
                    Console.WriteLine($"{author.FirstName} - {author.LastName} already exists");
                    throw new InvalidOperationException("Author already exists");

                }
            }
            var newAuthor = await _authorsRepository.CreateAuthorAsync(author);
            return newAuthor;

        }
        public async Task<bool> DeleteAuthor(Guid Id)
        {
            bool success = await _authorsRepository.DeleteAuthor(Id);
            if (success)
            {
                return true;
            }
            return false;
        }

        public async Task<List<Authors>> GetAuthorsAsync(AuthorsQuery query)
        {
            var authors = await _authorsRepository.GetAuthorsAsync(query);
            return authors;

        }
    }
}
