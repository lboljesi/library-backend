using LibraryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository.Common
{
    public interface ILoanRepository
    {
        Task<LoanResult> GetAllLoansAsync(int page, int pageSize, string? sortBy, string? sortDirection, string? search, bool? isReturned);
        Task<Loan?> GetLoanByIdAsync(Guid id);
        Task<Loan> AddLoanAsync(CreateLoanDto loan);
        Task UpdateLoanAsync (Guid id, LoanUpdateDTO loan);
        Task DeleteLoanAsync(Guid id);
    }
}
