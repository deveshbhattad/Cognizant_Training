
using Bloggie.MVC.Data;
using Bloggie.MVC.Models.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bloggie.MVC.Repositories

{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly BloggieDbContext bloggieDbContext;
        public BlogPostRepository(BloggieDbContext bloggieDbContext)
        {
            this.bloggieDbContext = bloggieDbContext;
        }
        [HttpPost]
        public async Task<BlogPost> AddAsync(BlogPost blogPost)
        {
            await bloggieDbContext.AddAsync(blogPost);
            await bloggieDbContext.SaveChangesAsync();
            return blogPost;
        }

        public async Task<BlogPost?> DeleteAsync(Guid id)
        {
            var exisitingBlogPosts = await bloggieDbContext.BlogPosts.FindAsync(id);
            if (exisitingBlogPosts != null)
            {
                 bloggieDbContext.Remove(exisitingBlogPosts);
                bloggieDbContext.SaveChangesAsync();
                return exisitingBlogPosts;
            }
            return null;
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            return await bloggieDbContext.BlogPosts.Include(x=> x.Tags).ToListAsync();
        }
        
        public async Task<BlogPost?> GetAsync(Guid id)
        {
            return await bloggieDbContext.BlogPosts.Include(x=>x.Tags).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BlogPost?> GetByUrlHandleAsync(string urlHandle)
        {
            return  await bloggieDbContext.BlogPosts.Include(x=>x.Tags).FirstOrDefaultAsync(x => x.Urlhandle == urlHandle);
                
        }

        public async Task<BlogPost?> UpdateAsync(BlogPost blogPost)
        {
            var existingblogPost = await bloggieDbContext.BlogPosts.Include(x => x.Tags).FirstOrDefaultAsync(x => x.Id == blogPost.Id);
            if (existingblogPost != null)
            {
                existingblogPost.Heading = blogPost.Heading;
                existingblogPost.PageTitle = blogPost.PageTitle;
                existingblogPost.Content = blogPost.Content;
                existingblogPost.ShortDescription = blogPost.ShortDescription;
                existingblogPost.FeaturedImageUrl = blogPost.FeaturedImageUrl;
                existingblogPost.Urlhandle = blogPost.Urlhandle;
                existingblogPost.PublishedDate = blogPost.PublishedDate;
                existingblogPost.Author = blogPost.Author;
                existingblogPost.Visible= blogPost.Visible;
                existingblogPost.Tags = blogPost.Tags;

                await bloggieDbContext.SaveChangesAsync();
                return existingblogPost;
            }
            return null;
        }
    }
}
