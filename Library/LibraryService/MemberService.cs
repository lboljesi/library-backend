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
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository)
        {
            _memberRepository = memberRepository;
        }
        public Task<List<MemberDto>> GetAllAsync()
        {
            return _memberRepository.GetAllAsync();
        }

        public Task<MemberDto?> GetByIdAsync(Guid id)
        {
            return _memberRepository.GetByIdAsync(id);
        }

        public Task<Guid> AddAsync(MemberCreateUpdateDto dto)
        {
            return _memberRepository.AddAsync(dto);
        }
        public Task<bool> UpdateAsync(Guid id, MemberCreateUpdateDto dto)
        {
            return _memberRepository.UpdateAsync(id, dto);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            return _memberRepository.DeleteAsync(id);
        }

    }
}
