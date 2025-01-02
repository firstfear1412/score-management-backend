using Microsoft.EntityFrameworkCore;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Entity
{
    public class scoreDB : DbContext
    {
        public scoreDB() { }
        public scoreDB(DbContextOptions<scoreDB> options):base(options){ }

        //***** DbSet *****//
        public DbSet<User> Users { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<SystemParam> SystemParams { get; set; }
        public DbSet<WebEvent_Logs> WebEvent_Logs { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<SubjectScore> SubjectScores { get; set; }
        public DbSet<EmailPlaceholder> EmailPlaceholders { get; set; }
        public DbSet<PlaceholderMapping> PlaceholderMappings { get; set; }
        public DbSet<UserEmailTemplate> UserEmailTemplates { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<UserDefaultEmailTemplate> UserDefaultEmailTemplates { get; set; }
        public DbSet<SubjectHeader> SubjectHeaders { get; set; }
        public DbSet<SubjectLecturer> SubjectLecturers { get; set; }

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
                .Entity<Subject>()
                .ToTable("Subject")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<Student>()
                .ToTable("Student")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<SubjectScore>()
                .ToTable("SubjectScore")
                .HasKey(a => new { a.sys_subject_no, a.student_id });
            modelBuilder
                .Entity<EmailPlaceholder>()
                .ToTable("EmailPlaceholder")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<PlaceholderMapping>()
                .ToTable("PlaceholderMapping")
                .HasKey(a => new { a.row_id });
            modelBuilder
                .Entity<UserEmailTemplate>()
                .ToTable("UserEmailTemplate")
                .HasKey(a => new { a.username, a.template_id });
            modelBuilder
                .Entity<EmailTemplate>()
                .ToTable("EmailTemplate")
                .HasKey(a => new { a.template_id });
            modelBuilder
                .Entity<UserDefaultEmailTemplate>()
                .ToTable("UserDefaultEmailTemplate")
                .HasKey(a => new { a.username, a.template_id });
            modelBuilder
                .Entity<SubjectHeader>()
                .ToTable("SubjectHeader")
                .HasKey(a => new { a.sys_subject_no });
            modelBuilder
                .Entity<SubjectLecturer>()
                .ToTable("SubjectLecturer")
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
