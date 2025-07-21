using LibraryModels;
using LibraryQuerying;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _service;
        public BookController(IBookService service)
        {
            _service = service;
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] SortablePaginationQuery query)
        {
            var result = await _service.GetBooksWithPaginationAsync(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] BooksCreateUpdate dto)
        {
            var id = await _service.AddBookAsync(dto);
            var book = await _service.GetBookByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, book);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BookDto>> GetById(Guid id)
        {
            var book = await _service.GetBookByIdAsync(id);
            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteBookAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPost("delete/bulk")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            var success = await _service.DeleteBookBulkAsync(ids);
            if (!success)
                return BadRequest("Failed to delete books.");

            return NoContent();
        }
    }
}
