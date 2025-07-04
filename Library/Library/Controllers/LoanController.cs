using LibraryModels;
using LibraryService.Common;
using Microsoft.AspNetCore.Mvc;

namespace Library.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoanController(ILoanService loanService)
        {
            _loanService = loanService;
        }


        [HttpGet]
        public async Task<ActionResult<LoanResult>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "LoanDate",
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] string? search = null,
        [FromQuery] bool? isReturned = null)
        {
            var result = await _loanService.GetAllLoansAsync(
                page, pageSize, sortBy, sortDirection, search, isReturned);

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetById(Guid id)
        {
            var loan = await _loanService.GetLoanByIdAsync(id);
            if (loan == null)
                return NotFound();

            return Ok(loan);
        }

    
        [HttpPost]
        public async Task<ActionResult<Loan>> Create([FromBody] CreateLoanDto loan)
        {
            var createdLoan = await _loanService.AddLoanAsync(loan);
            return CreatedAtAction(nameof(GetById), new { id = createdLoan.Id }, createdLoan);
        }

      
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LoanUpdateDTO loan)
        {
            await _loanService.UpdateLoanAsync(id, loan);
            return NoContent();
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _loanService.DeleteLoanAsync(id);
            return NoContent();
        }
    }
}
