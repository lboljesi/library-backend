using LibraryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootcampApp.Repository.Common
{
    public interface IMemberRepository
    {
        Task<MemberResult> GetAllMembersAsync(int page,int pageSize,string? sortBy, string? sortDirection, string? search, int? birthYear);
        Task<MemberREST?> GetMemberByIdAsync(Guid id);
        Task<Member> AddMemberAsync(Member member);
        Task UpdateMemberAsync(Guid id, Member member);
        Task DeleteMemberAsync(Guid id);
    }
}
