using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using Bloggie.MVC.Data;
using Bloggie.MVC.Models.Domain;
using Bloggie.MVC.Models.ViewModels;
using Bloggie.MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;

namespace Bloggie.MVC.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminBlogPostsController : Controller
    {
        public readonly ITagRepository tagRepository;
        public readonly IBlogPostRepository blogPostRepository;
        public AdminBlogPostsController(ITagRepository tagRepository,IBlogPostRepository blogPostRepository)
        {
            this.tagRepository = tagRepository;
            this.blogPostRepository = blogPostRepository;
        }

  

        [HttpGet]
        public async Task< IActionResult> Add()
        {
            var tags= await tagRepository.GetAllAsync();
            var model = new AddBlogPostsRequest
            {
                Tags = tags.Select(x => new SelectListItem { Text=x.Name, Value= x.Id.ToString()})
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Add(AddBlogPostsRequest addBlogPostsRequest)
        {
            var blogPost = new BlogPost //maps view model to domain model
            {
                Heading = addBlogPostsRequest.Heading,
                PageTitle = addBlogPostsRequest.PageTitle,
                Content = addBlogPostsRequest.Content,
                ShortDescription = addBlogPostsRequest.ShortDescription,
                FeaturedImageUrl = addBlogPostsRequest.FeaturedImageUrl,
                Urlhandle = addBlogPostsRequest.Urlhandle,
                PublishedDate = addBlogPostsRequest.PublishedDate,
                Author = addBlogPostsRequest.Author,
                Visible = addBlogPostsRequest.Visible
            };
            var selectedTags = new List<Tag>();
            foreach (var selectedTagId in addBlogPostsRequest.SelectedTags)
            {
                var selectedTagIdAsGuid = Guid.Parse(selectedTagId);
                var exisitingTag = await tagRepository.GetAsync(selectedTagIdAsGuid);
                
                if (exisitingTag != null)
                {
                    selectedTags.Add(exisitingTag);
                }
            }
            blogPost.Tags = selectedTags;
                await blogPostRepository.AddAsync(blogPost);
                return RedirectToAction("Add");
                 
        }
        public async Task<ActionResult> List()
        {
            var blogPosts=await blogPostRepository.GetAllAsync();
            return View(blogPosts);
        }
        [HttpGet]
        public async Task<ActionResult> Edit(Guid id)
        {
            var blogPost = await blogPostRepository.GetAsync(id);
            var tagsdomainmodel = await tagRepository.GetAllAsync();
            //map blogpost to addblogpostrequest
            if (blogPost != null)
            {
                var model = new EditBlogPostsRequest
                {
                    Id = blogPost.Id,
                    Heading = blogPost.Heading,
                    PageTitle = blogPost.PageTitle,
                    Content = blogPost.Content,
                    ShortDescription = blogPost.ShortDescription,
                    FeaturedImageUrl = blogPost.FeaturedImageUrl,
                    Urlhandle = blogPost.Urlhandle,
                    PublishedDate = blogPost.PublishedDate,
                    Author = blogPost.Author,
                    Visible = blogPost.Visible,
                    Tags = tagsdomainmodel.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                    SelectedTags=blogPost.Tags.Select(x=>x.Id.ToString()).ToArray()
                };
                return View(model);

            }
            return View(null);
         
        }
        [HttpPost]
        public async Task<ActionResult> Edit(EditBlogPostsRequest editBlogPostsRequest)

        {
            var blogpostdomainmodel = new BlogPost
            {
                Id = editBlogPostsRequest.Id,
                Heading = editBlogPostsRequest.Heading,
                PageTitle = editBlogPostsRequest.PageTitle,
                Content = editBlogPostsRequest.Content,
                ShortDescription = editBlogPostsRequest.ShortDescription,
                FeaturedImageUrl = editBlogPostsRequest.FeaturedImageUrl,
                Urlhandle = editBlogPostsRequest.Urlhandle,
                PublishedDate = editBlogPostsRequest.PublishedDate,
                Author = editBlogPostsRequest.Author,
                Visible = editBlogPostsRequest.Visible
            };
            var selectedTags = new List<Tag>();
            foreach(var selectedTag in editBlogPostsRequest.SelectedTags)
            {
                if(Guid.TryParse(selectedTag, out var tag)){
                    var foundTag = await tagRepository.GetAsync(tag);
                    if (foundTag != null)
                    {
                        selectedTags.Add(foundTag);
                    }
                }

            }
            blogpostdomainmodel.Tags = selectedTags;

            var updatedblogposts = await blogPostRepository.UpdateAsync(blogpostdomainmodel);
            if (updatedblogposts != null)
            {
                return RedirectToAction("Edit");
            }
            else
            {
                //show error message
                return RedirectToAction("Edit");
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAsync(EditBlogPostsRequest editBlogPostsRequest)
        {
            var deletedBlogPosts = await blogPostRepository.DeleteAsync(editBlogPostsRequest.Id);
            if (deletedBlogPosts != null)
            {
                return RedirectToAction("List");
            }
            return RedirectToAction("Edit");
        }
    }
}
