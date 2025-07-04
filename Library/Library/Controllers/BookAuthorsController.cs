using LibraryModels;
using LibraryRestModels;
using LibraryService;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookAuthorsController : ControllerBase
    {
        private readonly IBookAuthorsService _bookauthorsService;

        public BookAuthorsController(IBookAuthorsService bookauthorsService)
        {
            _bookauthorsService = bookauthorsService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]


        public async Task<ActionResult<List<BookAuthors>>> GetBookAuthorsAsync()
        {
            var bookAuthors = await _bookauthorsService.GetBookAuthorsAsync();

            if (bookAuthors == null)
            {
                return NotFound("No records for Book authors available");
            }


            return Ok(bookAuthors);



        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<BookAuthors>> GetBookAuthorsByIdAsync(Guid id)
        {
            var result = await _bookauthorsService.GetBookAuthorsByIdAsync(id);

            if (result == null)
            {
                return NotFound("No Book author with shared Id");
            }

            return Ok(result);



        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<BookAuthors>> UpdateBookAuthorAsync(Guid id, [FromBody] BookAuthorsCreateUpdateREST bookAuthor)
        {

            if (bookAuthor == null)
            {
                return BadRequest();
            }

            var domainModel = new BookAuthors
            {
                Id = id,
                BookId = bookAuthor.BookId,
                AuthorId = bookAuthor.AuthorId
            };


            var updatedBookAuthor = await _bookauthorsService.UpdateBookAuthorAsync(domainModel);

            if (updatedBookAuthor == null)
            {
                return NotFound("No book author find with the corresponding Id");
            }

            return Ok(updatedBookAuthor);





        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<BookAuthors>> CreateBookAuthorsAsync([FromBody] BookAuthorsCreateUpdateREST bookAuthors)
        {
            if (bookAuthors == null)
            {
                return BadRequest("Invalid Json body, body must not be empty");
            }

            var domainModel = new BookAuthors
            {
                BookId = bookAuthors.BookId,
                AuthorId = bookAuthors.AuthorId
            };

            var newbookAuthor = await _bookauthorsService.CreateBookAuthorsAsync(domainModel);

            return Ok(newbookAuthor);

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<bool>> DeleteBookAuthorsAsync(Guid id)
        {
            var delete = await _bookauthorsService.DeleteBookAuthorsAsync(id);

            if (!delete)
            {

                return NotFound("No record with the matching Id found");

            }
            return NoContent();


        }

        [HttpGet("books-by-author/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<List<Books>>> GetAllBooksByAuthorAsync(Guid id)
        {
            if (id == null)
            {
                return BadRequest();



            }
            var booksbyAuthor = await _bookauthorsService.GetAllBooksByAuthorAsync(id);
            if (!booksbyAuthor.Any())
            {
                return NoContent();

            }
            return Ok(booksbyAuthor);


        }


        [HttpGet("authors-by-book/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<Authors>>> GetAllAuthorsByBookAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }
            var authorsbyBook = await _bookauthorsService.GetAllAuthorsByBookAsync(id);

            if (!authorsbyBook.Any())
            {
                return NoContent();
            }
            return Ok(authorsbyBook);
        }



    }
}
