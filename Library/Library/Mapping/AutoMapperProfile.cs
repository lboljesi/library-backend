using AutoMapper;
using LibraryModels;
using LibraryRestModels;

namespace Library.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Category, CategoryREST>();
        }
    }
}
