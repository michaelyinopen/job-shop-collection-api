using AutoMapper;
using job_shop_collection_api.Data.Models;

namespace job_shop_collection_api.Dtos
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<JobSet, JobSetHeaderDto>();
            CreateMap<JobSet, JobSetDto>();
            CreateMap<NewJobSetRequest, JobSet>();
            CreateMap<UpdateJobSetRequest, JobSet>();
        }
    }
}
