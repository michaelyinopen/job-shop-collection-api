using job_shop_collection_api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace job_shop_collection_api.Data
{
    public class JobShopCollectionDbContext : DbContext
    {
        public JobShopCollectionDbContext(DbContextOptions<JobShopCollectionDbContext> options)
            : base(options)
        { }

        public DbSet<JobSet> JobSet { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JobSet>()
                .Property(e => e.RowVersion)
                .IsRowVersion();
        }
    }
}
