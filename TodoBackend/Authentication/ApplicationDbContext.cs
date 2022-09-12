using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoBackend.Models;

namespace TodoBackend.Authentication
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<TagModel> Tag { get; set; }
        public DbSet<TodoModel> Todo { get; set; }
        public DbSet<PostModel> Post { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<TagModel>().ToTable("Tag");
            builder.Entity<TodoModel>().ToTable("Todo");
            builder.Entity<PostModel>().ToTable("Post");
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "User");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Role");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");   
            });

            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });
            builder.Entity<ApplicationUser>()
               .HasMany(p => p.Todos);
            builder.Entity<ApplicationUser>()
               .HasMany(p => p.Posts);
            builder.Entity<PostModel>()
               .HasMany(p => p.Tags);
            builder.Entity<PostModel>()
               .Property(b => b.CreatedDate)
               .HasDefaultValueSql("getdate()");
        }
    }
}
