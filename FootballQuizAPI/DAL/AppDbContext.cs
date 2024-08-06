using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using FootballQuizAPI.Models;

namespace FootballQuizAPI.DAL
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<Choice> Choices { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>()
              .HasOne(q => q.Category)
              .WithMany(c => c.Questions)
              .HasForeignKey(q => q.CategoryId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizResult>()
              .HasOne(q => q.User)
              .WithMany(u => u.QuizResults)
              .HasForeignKey(q => q.UserId)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
            .HasMany(q => q.Choices)
            .WithOne(c => c.Question)
            .HasForeignKey(c=>c.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                  new Role { Id = 2, Name = "User", NormalizedName = "USER" }
              );
            var hasher = new PasswordHasher<User>();
            modelBuilder.Entity<User>().HasData(
              new User
              {
                  Id = 1,
                  UserName = "ilkin.admin",
                  XP = 100,
                  Level= 1,
                  CountryCode="az",
                  PasswordHash = hasher.HashPassword(null, "Admin.1234"),
                  Email = "inovruzov2004@gmail.com",
                  EmailConfirmed = true,
                  NormalizedUserName = "ILKIN.ADMIN",
                  NormalizedEmail = "INOVRUZOV2004@GMAIL.COM",
                  LockoutEnabled = true,
                  SecurityStamp = Guid.NewGuid().ToString()
              });

            modelBuilder.Entity<IdentityUserRole<int>>().HasData(new IdentityUserRole<int> { UserId = 1, RoleId = 1 });

            base.OnModelCreating(modelBuilder);
        }
    }
}
