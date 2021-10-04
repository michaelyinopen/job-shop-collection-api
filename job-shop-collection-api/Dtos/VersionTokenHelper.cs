using job_shop_collection_api.Data.Models;
using System;

namespace job_shop_collection_api.Dtos
{
    public static class VersionTokenHelper
    {
        public static string? GetVersionToken(byte[]? rowVersion) => rowVersion is null ? null : Convert.ToBase64String(rowVersion);
        public static byte[]? ConvertToRowVersion(string versionToken) => Convert.FromBase64String(versionToken);
        public static string? GetVersionToken(JobSet jobSet) => GetVersionToken(jobSet.RowVersion);
        public static string? GetVersionToken(JobSetDto jobSetDto) => GetVersionToken(jobSetDto.RowVersion);
    }
}
