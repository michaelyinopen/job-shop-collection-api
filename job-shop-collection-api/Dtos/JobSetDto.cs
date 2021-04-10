using Newtonsoft.Json;
using System.Collections.Generic;

namespace job_shop_collection_api.Dtos
{
    public class JobSetHeaderDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public bool IsLocked { get; set; }

        [JsonIgnore]
        public byte[]? RowVersion { get; set; }

        [JsonProperty]
        public string? ETag => EtagHelper.GetETag(RowVersion);
    }

    public class JobSetHeadersDto
    {
        public List<JobSetHeaderDto> Data { get; set; } = new List<JobSetHeaderDto>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NextPageToken { get; set; }
    }


    public class JobSetDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? JobColors { get; set; }

        public bool IsAutoTimeOptions { get; set; }

        public string? TimeOptions { get; set; }

        public bool IsLocked { get; set; }

        [JsonIgnore]
        public byte[]? RowVersion { get; set; }

        [JsonProperty]
        public string? ETag => EtagHelper.GetETag(RowVersion);
    }

    public class NewJobSetDto
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? JobColors { get; set; }

        public bool IsAutoTimeOptions { get; set; }

        public string? TimeOptions { get; set; }
    }

    public class UpdateJobSetDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? JobColors { get; set; }

        public bool IsAutoTimeOptions { get; set; }

        public string? TimeOptions { get; set; }
    }
}
