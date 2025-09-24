using LibraryModels;
using LibraryQuerying;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;
        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var members = await _memberService.GetAllAsync();
            return Ok(members);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
                return NotFound();

            return Ok(member);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MemberCreateUpdateDto dto)
        {
            var memberId = await _memberService.AddAsync(dto);
            var createdMember = await _memberService.GetByIdAsync(memberId);
            return CreatedAtAction(nameof(GetById), new { id = memberId }, createdMember);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, MemberCreateUpdateDto dto)
        {
            var updated = await _memberService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _memberService.DeleteAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] SortablePaginationQuery q)
        {
            var result = await _memberService.GetPagedAsync(q);
            return Ok(result);
        }
    }
}
