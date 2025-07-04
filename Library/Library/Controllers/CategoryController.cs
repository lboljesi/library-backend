using AutoMapper;
using LibraryModels;
using LibraryQuerying;
using LibraryRestModels;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly IMapper _mapper;
        public CategoryController(ICategoryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<Category>>> Get([FromQuery] PaginationQuery query)
        {
            var pagedResult = await _service.GetPagedCategoriesAsync(query);
            var mapped = new PagedResult<CategoryREST>
            {
                Items = _mapper.Map<List<CategoryREST>>(pagedResult.Items),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            };
            return Ok(mapped);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryREST>> GetById(Guid id)
        {
            var category = await _service.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();
            var mapped = _mapper.Map<CategoryREST>(category);
            return Ok(mapped);
        }

        [HttpPost]
        public async Task<ActionResult<Category?>> Create([FromBody] CategoryAdd newCategory)
        {
            if (string.IsNullOrWhiteSpace(newCategory.Name))
                return BadRequest("Category name is required.");
            var domainModel = new Category
            {
                Id = Guid.NewGuid(),
                Name = newCategory.Name
            };
            try
            {
                var category = await _service.CreateCategoryAsync(domainModel);

                if (category == null) // Ensure category is not null before dereferencing
                    return Problem("An unexpected error occurred while creating the category.");

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<Category?>> Update(Guid id, [FromBody] CategoryREST updatedCategory)
        {
            if (updatedCategory == null)
            {
                return BadRequest("Request body must be provided.");
            }

            var domainModel = new Category
            {
                Id = id,
                Name = updatedCategory.Name
            };

            var category = await _service.UpdateCategoryAsync(domainModel);
            if (category == null)
                return NotFound($"No category found to update with Id - {id}");

            var mapped = _mapper.Map<CategoryREST>(category);
            return Ok(mapped);

        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(Guid id)
        {
            var deleted = await _service.DeleteCategoryAsync(id);
            if (!deleted)
                return NotFound();
            return Ok("Category deleted successfully!");
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCount([FromQuery] PaginationQuery query)
        {
            var total = await _service.GetTotalCountAsync(query);
            return Ok(total);
        }

        [HttpGet("with-books")]
        public async Task<ActionResult<List<CategoryWithBooks>>> GetCategoriesWithBooks()
        {
            var result = await _service.GetCategoriesWithBooksAsync();
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<Category>>> GetAll()
        {
            return Ok(await _service.GetAllCategoriesAsync());
        }

    }
}
