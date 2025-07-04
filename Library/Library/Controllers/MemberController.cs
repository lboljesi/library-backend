
using LibraryModels;
using LibraryService;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet]

        [HttpGet]
        public async Task<ActionResult<MemberResult>> GetAllMembers(
            int page = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? sortDirection = "asc",
            string? search = null,
            int? birthYear = null)

            {
                var result = await _memberService.GetAllMembersAsync(page, pageSize, sortBy, sortDirection, search, birthYear);
                return Ok(result);
            }

        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMemberById(Guid id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
                return NotFound();

            return Ok(member);
        }

        [HttpPost]
        public async Task<ActionResult<Member>> AddMember([FromBody] Member member)
        {
            var created = await _memberService.AddMemberAsync(member);
            return CreatedAtAction(nameof(GetMemberById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMember(Guid id, [FromBody] Member member)
        {
            await _memberService.UpdateMemberAsync(id, member);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            await _memberService.DeleteMemberAsync(id);
            return NoContent();
        }
    }
}
