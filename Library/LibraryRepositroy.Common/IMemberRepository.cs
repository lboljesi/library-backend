using LibraryModels;
using LibraryQuerying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryRepository.Common
{
    public interface IMemberRepository
    {
        Task<List<MemberDto>> GetAllAsync();
        Task<MemberDto?> GetByIdAsync(Guid id);

        Task<Guid> AddAsync(MemberCreateUpdateDto dto);
        Task<bool> UpdateAsync(Guid id, MemberCreateUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<PagedResultMember<MemberDto>> GetPagedAsync(SortablePaginationQuery q);

    }
}
