using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        public string? VersionToken => VersionTokenHelper.GetVersionToken(RowVersion);
    }

    public class JobSetsQuery
    {
        public int Limit { get; set; } = 100;

        public int? PageToken { get; set; }
    }

    public class JobSetsResponse
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
        public string? VersionToken => VersionTokenHelper.GetVersionToken(RowVersion);
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum GetJobSetResponseStatus
    {
        [EnumMember(Value = "ok")]
        Ok,

        [EnumMember(Value = "not found")]
        NotFound,
    }

    public class GetJobSetResponse
    {
        public GetJobSetResponseStatus Status { get; set; }

        public JobSetDto? Data { get; set; }
    }

    public class NewJobSetRequest
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? JobColors { get; set; }

        public bool IsAutoTimeOptions { get; set; }

        public string? TimeOptions { get; set; }
    }

    public class UpdateJobSetRequest
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? Content { get; set; }

        public string? JobColors { get; set; }

        public bool IsAutoTimeOptions { get; set; }

        public string? TimeOptions { get; set; }

        public string? VersionToken { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum UpdateJobSetResponseStatus
    {
        [EnumMember(Value = "not found")]
        NotFound,

        [EnumMember(Value = "version condition failed")]
        VersionConditionFailed,

        [EnumMember(Value = "forbidden because locked")]
        ForbiddenBeacuseLocked,

        [EnumMember(Value = "done")]
        Done,
    }

    public class UpdateJobSetResponse
    {
        public UpdateJobSetResponseStatus Status { get; set; }

        public JobSetDto? SavedJobSet { get; set; }

        public JobSetDto? UpdatedJobSet { get; set; }
    }
}
