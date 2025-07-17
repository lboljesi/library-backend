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
    }
}
