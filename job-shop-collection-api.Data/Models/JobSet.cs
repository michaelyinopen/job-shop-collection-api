using System.ComponentModel.DataAnnotations;

namespace job_shop_collection_api.Data.Models
{
    public class JobSet
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Required]
        public string? Content { get; set; }

        public string? JobColors { get; set; }

        public bool IsAutoTimeOptions { get; set; }

        public string? TimeOptions { get; set; }

        public bool IsLocked { get; set; }

        public byte[]? RowVersion { get; set; }
    }
}
