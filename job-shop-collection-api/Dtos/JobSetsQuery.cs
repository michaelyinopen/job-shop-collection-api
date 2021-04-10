namespace job_shop_collection_api.Dtos
{
    public class JobSetsQuery
    {
        public int Limit { get; set; } = 100;

        public int? PageToken { get; set; }
    }
}
