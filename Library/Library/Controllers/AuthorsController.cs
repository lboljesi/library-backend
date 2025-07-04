using LibraryModels;
using LibraryQuerying;
using LibraryRestModels;
using LibraryService;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorsService _authorsService;

        public AuthorsController(IAuthorsService authorsService)
        {
            _authorsService = authorsService;
        }

        // GET: api/Authors
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<AuthorsREST>>> GetAuthorsAsync([FromQuery] AuthorsQuery query)
        {
            var authors = await _authorsService.GetAuthorsAsync(query);

            var result = authors.Select(a => new AuthorsREST
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                BookTitles = a.BookTitles
            });

            return Ok(result);
        }

        // GET: api/Authors/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorsREST>> GetAuthorByIdAsync(Guid id)
        {
            var author = await _authorsService.GetAuthorByIdAsync(id);

            if (author == null)
            {
                return NotFound("No author with the matching Id found.");
            }

            var result = new AuthorsREST
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };

            return Ok(result);
        }

        // PUT: api/Authors/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorsREST>> UpdateAuthorAsync(Guid id, [FromBody] AuthorsUpdateCreateREST updatedAuthor)
        {
            if (updatedAuthor == null)
            {
                return BadRequest("Request body must be provided.");
            }

            var domainModel = new Authors
            {
                Id = id,
                FirstName = updatedAuthor.FirstName,
                LastName = updatedAuthor.LastName
            };

            var author = await _authorsService.UpdateAuthorAsync(domainModel);

            if (author == null)
            {
                return NotFound($"No book found to update with Id - {id}");
            }

            var result = new AuthorsREST
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName

            };

            return Ok(result);
        }

        // POST: api/Authors
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthorsREST>> CreateBookAsync([FromBody] AuthorsUpdateCreateREST newAuthor)
        {
            if (newAuthor == null)
            {
                return BadRequest("Request body for the new book must be provided.");
            }

            var domainModel = new Authors
            {
               
               FirstName = newAuthor.FirstName,
               LastName = newAuthor.LastName
            };
            try
            {
                var author = await _authorsService.CreateAuthorAsync(domainModel);

                var authorREST = new AuthorsREST
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                };
                return Ok(authorREST);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(new { message = ex.Message });
            }

            //var bookResult = _booksService.GetBookByIdAsync(bookREST.Id);

            
        }

        // DELETE: api/Authors/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuthor(Guid id)
        {
            var success = await _authorsService.DeleteAuthor(id);
            if (!success)
            {
                return NotFound($"No author found with Id - {id}");
            }

            return NoContent();
        }
    }
}