using Microsoft.EntityFrameworkCore;
using ScoreManagement.Model.Table.Subject;
using ScoreManagement.Model.Table.Student;
using ScoreManagement.Model.Table.User;
using ScoreManagement.Model.Table.SubjectScore;
using ScoreManagement.Model.Table.Language;
using ScoreManagement.Model.Table.SystemParam;
using ScoreManagement.Model.Table.WebEvent;

namespace ScoreManagement.Entity
{
    public class demoDB : DbContext
    {
        public demoDB() { }
        public demoDB(DbContextOptions<demoDB> options):base(options){ }

        //***** DbSet *****//
        public DbSet<User> Users { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<SystemParam> SystemParams { get; set; }
        public DbSet<WebEvent_Logs> WebEvent_Logs { get; set; }
        public DbSet<SubjectResource> Subjects { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<SubjectScoreResource> SubjectScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
               .Entity<User>()
               .ToTable("User")
               .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<Language>()
                .ToTable("Language")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<SystemParam>()
                .ToTable("SystemParam")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<WebEvent_Logs>()
                .ToTable("WebEvent_Logs")
                .HasKey(a => new { a.event_id });
            modelBuilder
                .Entity<SubjectResource>()
                .ToTable("Subject")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<Student>()
                .ToTable("Student")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<SubjectScoreResource>()
                .ToTable("SubjectScore")
                .HasKey(a => new { a.row_id });
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(true, cancellationToken);
        }
    }
}
