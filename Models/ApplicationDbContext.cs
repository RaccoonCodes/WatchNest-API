using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WatchNest.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApiUsers>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<SeriesModel> Series => Set<SeriesModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //ApiUsers
            modelBuilder.Entity<ApiUsers>(entity =>
            {
                entity.HasKey(k => k.Id);
                entity.Property(p => p.Id).HasColumnName("UserId");

                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.UserName);
            });

            //SeriesModel
            modelBuilder.Entity<SeriesModel>(entity =>
            {
                entity.HasKey(k => k.SeriesID);
                entity.Property(P => P.SeriesID).ValueGeneratedOnAdd(); //Auto Generate ID
                entity.HasOne(h => h.ApiUsers) //One to many Relationship
                .WithMany(u => u.Series)
                .HasForeignKey(u => u.UserID)
                .OnDelete(DeleteBehavior.Cascade);

                entity.Property(p => p.RowVersion).IsRowVersion();

                //Index for query performance
                entity.HasIndex(e => e.UserID);
                entity.HasIndex(e => e.TitleWatched);
                entity.HasIndex(e => e.Genre);
                entity.HasIndex(e => e.SeriesID);
            });
        }
    }
}
