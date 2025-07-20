using LibraryModels;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        
        private readonly IAuthorService _service;
        public AuthorController(IAuthorService service)
        {
            _service = service;
        }
        
        [HttpGet("all")]
        public async Task<ActionResult<List<AuthorDto>>> GetAll()
        {
            var authors = await _service.GetAllAsync();
            return Ok(authors);
        }

    }
}
