using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostAttachment> PostAttachments { get; set; }
        public DbSet<RegistrationConfig> RegistrationConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Student Relations ──
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Level)
                .WithMany(l => l.Students)
                .HasForeignKey(s => s.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Division)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.DivisionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.NationalId)
                .IsUnique();

            // ── Post Relations ──
            modelBuilder.Entity<PostAttachment>()
                .HasOne(a => a.Post)
                .WithMany(p => p.Attachments)
                .HasForeignKey(a => a.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Seed Data ──
            modelBuilder.Entity<Level>().HasData(
                new Level { Id = 1, Name = "الأول" },
                new Level { Id = 2, Name = "الثاني" },
                new Level { Id = 3, Name = "الثالث" }
            );

            modelBuilder.Entity<Division>().HasData(
                new Division { Id = 1, Name = "الحاسب الالى " },
                new Division { Id = 2, Name = "الرياضيات " },
                new Division { Id = 3, Name = "الكيمياء" },
                new Division { Id = 4, Name = "الفيزياء" },
                new Division { Id = 5, Name = "العلوم البيولوجية والجيولوجية والبيئية" },
                new Division { Id = 6, Name = "اللغة الانجليزية" },
                new Division { Id = 7, Name = "اللغة العربية" },
                new Division { Id = 8, Name = "اللغة الفرنسية" },
                new Division { Id = 9, Name = "التاريخ" },
                new Division { Id = 10, Name = "الجغرافيا" },
                new Division { Id = 11, Name = "علم النفس" },
                new Division { Id = 12, Name = "الدراسات الاجتماعية - تعليم اساسي" },
                new Division { Id = 13, Name = "الرياضيات - تعليم اساسي" },
                new Division { Id = 14, Name = "اللغة العربية - تعليم اساسي" },
                new Division { Id = 15, Name = "اللغة الانجليزية - تعليم اساسي" },
                new Division { Id = 16, Name = "علوم - تعليم اساسي" }
            );

            modelBuilder.Entity<AdminUser>().HasData(
                new AdminUser
                {
                    Id = 1,
                    Username = "admin",
                    // سيتم تعيين كلمة المرور عند بدء التشغيل من متغير البيئة
                    Password = "" // فارغ مؤقتاً، سيتم تعيينه في Program.cs
                }
            );

            modelBuilder.Entity<RegistrationConfig>().HasData(
                new RegistrationConfig
                {
                    Id = 1,
                    IsOpen = true,
                    ClosedMessage = "التسجيل مغلق حالياً. يرجى متابعة الإعلانات للاطلاع على مواعيد التسجيل."
                }
            );
        }
    }
}
