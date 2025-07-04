using LibraryModels;
using LibraryQuerying;
using LibraryRestModels;
using LibraryService;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBooksService _booksService;

        public BooksController(IBooksService booksService)
        {
            _booksService = booksService;
        }

        // GET: api/Books
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BooksREST>>> GetBooksAsync([FromQuery] BooksQuery query)
        {
            var books = await _booksService.GetBooksAsync(query);

            var result = books.Select(b => new BooksREST
            {
                Id = b.Id,
                Title = b.Title,
                Isbn = b.Isbn,
                PublishedYear = b.PublishedYear,
                Price = b.Price,
                BookAuthors = b.BookAuthors
            });

            return Ok(result);
        }

        // GET: api/Books/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BooksREST>> GetBookByIdAsync(Guid id)
        {
            var book = await _booksService.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound("No book with the matching Id found.");
            }

            var result = new BooksREST
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                PublishedYear = book.PublishedYear,
                Price = book.Price
            };

            return Ok(result);
        }

        // PUT: api/Books/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BooksREST>> UpdateBookAsync(Guid id, [FromBody] BooksCreateUpdateREST updatedBook)
        {
            if (updatedBook == null)
            {
                return BadRequest("Request body must be valid.");
            }

            var domainModel = new Books
            {
                Id = id,
                Title = updatedBook.Title,
                Isbn = updatedBook.Isbn,
                PublishedYear = updatedBook.PublishedYear,
                Price = updatedBook.Price
            };

            var book = await _booksService.UpdateBookAsync(domainModel);

            if (book == null)
            {
                return NotFound($"No book found to update with Id - {id}");
            }

            var result = new BooksREST
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                PublishedYear = book.PublishedYear,
                Price = book.Price
            };

            return Ok(result);
        }

        // POST: api/Books
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BooksREST>> CreateBookAsync([FromBody] BooksCreateUpdateREST newBook)
        {
            if (newBook == null)
            {
                return BadRequest("Request body for the new book must be provided.");
            }

            var domainModel = new Books
            {
                Title = newBook.Title,
                Isbn = newBook.Isbn,
                PublishedYear = newBook.PublishedYear,
                Price = newBook.Price
            };

            try
            {

                var book = await _booksService.CreateBookAsync(domainModel);

                var bookREST = new BooksREST
                {
                    Id = book.Id,
                    Title = book.Title,
                    Isbn = book.Isbn,
                    PublishedYear = book.PublishedYear,
                    Price = book.Price
                };
                return Ok(bookREST);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(new { message = ex.Message });
            }

            //var bookResult = _booksService.GetBookByIdAsync(bookREST.Id);

            
        }

        // DELETE: api/Books/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _booksService.DeleteBook(id);
            if (!success)
            {
                return NotFound($"No book found with Id - {id}");
            }

            return NoContent();
        }
    }
}