using BootcampApp.Repository.Common;
using LibraryModels;
using LibraryRepository;
using LibraryService.Common;

namespace LibraryService
{
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _repository;

        public MemberService(IMemberRepository repository)
        {
            _repository = repository;
        }

        public async Task<MemberResult> GetAllMembersAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortDirection = "asc",
        string? search = null,
        int? birthYear = null
    )
        {
            return await _repository.GetAllMembersAsync(page, pageSize, sortBy, sortDirection, search, birthYear);
        }

        public async Task<MemberREST> GetMemberByIdAsync(Guid id)
        {
            return await  _repository.GetMemberByIdAsync(id);
        }

        public async Task<Member> AddMemberAsync(Member member)
        {
            return await _repository.AddMemberAsync(member);
        }

        public async Task UpdateMemberAsync(Guid id, Member member)
        {
            await _repository.UpdateMemberAsync(id, member);
        }

        public async Task DeleteMemberAsync(Guid id)
        {
            await _repository.DeleteMemberAsync(id);
        }
    }
}
