using job_shop_collection_api.Data.Models;
using System;

namespace job_shop_collection_api.Dtos
{
    public static class EtagHelper
    {
        public static string? GetETag(byte[]? rowVersion) => rowVersion is null ? null : Convert.ToBase64String(rowVersion);
    }

    public static class EtagExtensions
    {
        public static string? GetETag(this JobSet jobSet) => EtagHelper.GetETag(jobSet.RowVersion);
        public static string? GetETag(this JobSetDto jobSetDto) => EtagHelper.GetETag(jobSetDto.RowVersion);
    }
}
