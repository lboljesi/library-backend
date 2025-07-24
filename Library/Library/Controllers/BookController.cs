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
        public async Task<IActionResult> GetPaged([FromQuery] BookQuery query)
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

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBook(Guid id, [FromBody] BookUpdateDto dto)
        {
            var success = await _service.UpdateBookAsync(id, dto);
            if (!success)
                return NotFound();

            var book = await _service.GetBookByIdAsync(id);
            return Ok(book);
        }

        [HttpGet("{id:guid}/authors")]
        public async Task<IActionResult> GetAllAuthorsByBookId(Guid id)
        {
            var result = await _service.GetAuthorsByBookIdAsync(id);
            return Ok(result);
        }

        [HttpGet("{id:guid}/categories")]
        public async Task<IActionResult> GetAllCategoriesByBookId(Guid id)
        {
            var result = await _service.GetCategoriesByBookIdAsync(id)
            return Ok(result);
        }
    }
}
