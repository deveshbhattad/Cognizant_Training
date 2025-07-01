using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bloggie.MVC.Data
{
    public class AuthDbContext : IdentityDbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var adminroleId = "5f41b0d0-eaac-4e7d-8bd5-e2dacf9cbddd";
            var superadminroleId = "723a175a-0840-4db8-877d-72ae8a1b0b22";
            var userroleId = "86699ef2-3292-42f7-99ec-df7b822a3815";
            var superadminId = "b5f60def-4923-4c00-85fb-7dbcbd5b0ca3";

            // Use a static password hash (generated once and pasted here)
            var superadminPasswordHash = "AQAAAAIAAYagAAAAEPJUYy9IBUwb3EkJh4luMFB70+HVRJ4sf2p940Bm424UU0woxIRyK40Sw5KQ3FAh+w==";
            var superadminConcurrencyStamp = "74f7916f-77ef-4607-9d77-59a0fb334e5a"; // Use the value from your migration

            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    Id = adminroleId,
                    ConcurrencyStamp = adminroleId
                },
                new IdentityRole
                {
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    Id = superadminroleId,
                    ConcurrencyStamp = superadminroleId
                },
                new IdentityRole
                {
                    Name = "user",
                    NormalizedName = "USER",
                    Id = userroleId,
                    ConcurrencyStamp = userroleId
                }
            );

            builder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = superadminId,
                    UserName = "superadmin@bloggie.com",
                    NormalizedUserName = "SUPERADMIN@BLOGGIE.COM",
                    Email = "superadmin@bloggie.com",
                    NormalizedEmail = "SUPERADMIN@BLOGGIE.COM",
                    EmailConfirmed = false,
                    PasswordHash = superadminPasswordHash,
                    SecurityStamp = string.Empty,
                    ConcurrencyStamp = superadminConcurrencyStamp, // <-- Now static
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0
                }
            );

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = superadminId, RoleId = adminroleId },
                new IdentityUserRole<string> { UserId = superadminId, RoleId = superadminroleId },
                new IdentityUserRole<string> { UserId = superadminId, RoleId = userroleId }
            );
        }
    }
}
