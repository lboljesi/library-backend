using LibraryModels;
using LibraryQuerying;
using LibraryRestModels;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookCategoryController : ControllerBase
    {
        private readonly IBookCategoryService _service;
        public BookCategoryController(IBookCategoryService service)
        {
            _service = service;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var deleted = await _service.DeleteBookCategoryAsync(id);
            if (!deleted)
                return NotFound("No such relation Found.");

            return NoContent();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookCategory>> Create([FromBody] CreateBookCategoryDto dto)
        {
            if (dto.BookId == Guid.Empty || dto.CategoryId == Guid.Empty)
                return BadRequest("BookId and CategoryId are required.");

            var created = await _service.CreateBookCategoryAsync(dto);
            if (created is null)
                return BadRequest("The relationship between the book and the category already exists or the provided IDs are invalid.");

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookCategory?>> GetById(Guid id)
        {
            var bookCategory = await _service.GetByIdAsync(id);
            if (bookCategory == null)
                return NotFound($"No BookCategory found with ID {id}");
            return Ok(bookCategory);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<BookCategoryJOIN>>> Get([FromQuery] SortablePaginationQuery query)
        {
            var result = await _service.GetAllAsync(query);
            if (result.Count == 0)
                return NotFound("No book-category relations found");
            return Ok(result);
        }

        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CategoryWithLinkDto>>> CreateManyAsync([FromBody] AddCategoriesToBookDto dto)
        {
            if (dto.CategoryIds == null || !dto.CategoryIds.Any())
                return BadRequest("At least one category must be selected.");

            try
            {
                var result = await _service.CreateManyAsync(dto.BookId, dto.CategoryIds);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("Book does not exist"))
                    return NotFound(ex.Message);

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
            }


        }
        [HttpGet("{categoryId}/books")]
        public async Task<ActionResult<List<Books>>> GetBooksForCategory(Guid categoryId)
        {
            var books = await _service.GetBooksForCategoryAsync(categoryId);

            return Ok(books);
        }

        [HttpGet("grouped-by-book")]
        public async Task<ActionResult<PagedResult<BookWithCategoriesDto>>> GetGroupedByBook([FromQuery] PaginationQuery query)
        {
            var result = await _service.GetGroupedByBooksAsync(query);
            return Ok(result);
        }

        [HttpGet("grouped-by-book/count")]
        public async Task<ActionResult<int>> CountGroupedBook([FromQuery] PaginationQuery query)
        {
            return Ok(await _service.CountGroupedBookAsync(query));
        }

        [HttpGet("empty")]
        public async Task<ActionResult<List<Category>>> GetEmptyCategories()
        {
            var result = await _service.GetCategoriesWithoutBooksAsync();
            return Ok(result);
        }
        [HttpPost("delete/by-relation-ids")]
        public async Task<IActionResult> DeleteRelations([FromBody] DeleteRelationIdList dto)
        {
            if (dto.RelationIds == null || dto.RelationIds.Count == 0)
                return BadRequest("No relation IDs provided.");

            var deletedCount = await _service.DeleteRelationsAsync(dto.RelationIds);
            if (deletedCount == 0)
                return NotFound("No matching relations found to delete!");
            return NoContent();
        }

        [HttpGet("{categoryId}/books-with-authors")]
        public async Task<IActionResult> GetBooksWithAuthorsForCategory(Guid categoryId)
        {
            var books = await _service.GetBooksWithAuthorsForCategoryAsync(categoryId);
            return Ok(books);
        }
    }
}
