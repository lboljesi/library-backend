using LibraryModels;
using LibraryRepository.Common;
using LibraryService.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _repository;

        public LoanService(ILoanRepository repository)
        {
            _repository = repository;
        }

        public async Task<LoanResult> GetAllLoansAsync(
            int page = 1,
            int pageSize = 10,
            string? sortBy = "LoanDate",
            string? sortDirection = "asc",
            string? search = null,
            bool? isReturned = null)
        {
            return await _repository.GetAllLoansAsync(page, pageSize, sortBy, sortDirection, search, isReturned);
        }

        public async Task<Loan?> GetLoanByIdAsync(Guid id)
        {
            return await _repository.GetLoanByIdAsync(id);
        }

        public async Task<Loan> AddLoanAsync(CreateLoanDto loan)
        {
            return await _repository.AddLoanAsync(loan);
        }

        public async Task UpdateLoanAsync(Guid id, LoanUpdateDTO loan)
        {
            await _repository.UpdateLoanAsync(id, loan);
        }

        public async Task DeleteLoanAsync(Guid id)
        {
            await _repository.DeleteLoanAsync(id);
        }
    }

}
