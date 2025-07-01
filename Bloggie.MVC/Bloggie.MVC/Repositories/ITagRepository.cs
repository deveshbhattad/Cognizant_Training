using Bloggie.MVC.Models.Domain;
namespace Bloggie.MVC.Repositories
{
    public interface ITagRepository
    {
        //only have defination of methods not implementation
         Task<IEnumerable<Tag>>GetAllAsync(); //getting all tags
        Task<Tag?> GetAsync(Guid id); //getting a single tag
        Task<Tag> AddSync(Tag tag);
        Task<Tag?> UpdateSync(Tag tag);
        Task<Tag?> DeleteSync(Guid id);
    }
}
