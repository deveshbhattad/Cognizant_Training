using Bloggie.MVC.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Bloggie.MVC.Data
{
    public class BloggieDbContext:DbContext
    {
        public BloggieDbContext(DbContextOptions<BloggieDbContext> options) : base(options)
        {
        }
        public DbSet<BlogPost>BlogPosts { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}
