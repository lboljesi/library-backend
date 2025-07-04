using LibraryModels;


namespace LibraryService.Common
{
    public interface ILoanService
    {
        Task<LoanResult> GetAllLoansAsync(int page = 1,
            int pageSize = 10,
            string? sortBy = "LoanDate",
            string? sortDirection = "asc",
            string? search = null,
            bool? isReturned = null);
        Task<Loan?> GetLoanByIdAsync(Guid id);
        Task<Loan> AddLoanAsync(CreateLoanDto loan);
        Task UpdateLoanAsync(Guid id, LoanUpdateDTO loan);
        Task DeleteLoanAsync(Guid id);
    }
}
