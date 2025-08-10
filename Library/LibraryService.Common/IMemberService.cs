using LibraryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.Common
{
    public interface IMemberService
    {
        Task<List<MemberDto>> GetAllAsync();
        Task<MemberDto?> GetByIdAsync(Guid id);
        Task<Guid> AddAsync(MemberCreateUpdateDto dto);
        Task<bool> UpdateAsync(Guid id, MemberCreateUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
