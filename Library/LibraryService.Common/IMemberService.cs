using LibraryModels;

namespace LibraryService.Common
{
    public interface IMemberService
    {
        Task<MemberResult> GetAllMembersAsync(int page = 1, int pageSize = 10, string? sortBy = null, string? sortDirection = "asc", string? search = null, int ? birthYear = null);
        Task<MemberREST> GetMemberByIdAsync(Guid id);
        Task<Member> AddMemberAsync(Member member);
        Task UpdateMemberAsync(Guid id, Member member);
        Task DeleteMemberAsync(Guid id);
    }
}
