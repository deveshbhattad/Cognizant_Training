using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bloggie.MVC.Models.ViewModels
{
    public class AddBlogPostsRequest
    {
        public Guid Id { get; set; }
        public string Heading { get; set; }
        public string PageTitle { get; set; }

        public string Content { get; set; }
        public string ShortDescription { get; set; }
        public string FeaturedImageUrl { get; set; }
        public string Urlhandle { get; set; }

        public DateTime PublishedDate { get; set; }

        public string Author { get; set; }
        public bool Visible { get; set; }

        public IEnumerable<SelectListItem> Tags { get; set; } //display tags
        public string[] SelectedTags { get; set; } = Array.Empty<string>(); //collect tags
    }
}
