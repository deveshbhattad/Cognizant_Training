using Bloggie.MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Bloggie.MVC.Data;
using Bloggie.MVC.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Bloggie.MVC.Repositories;
using Microsoft.AspNetCore.Authorization;
namespace Bloggie.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTagsController : Controller
    {
        public ITagRepository tagRepository { get; }

        public AdminTagsController(ITagRepository tagRepository) {
            this.tagRepository = tagRepository;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Add()
        {
           
            return View();
           
        }
        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> Add(AddTagRequest addTagRequest)
        {
            //mapping addtagrequest to models.domain.tag
            var tag = new Tag
            {
                Name = addTagRequest.Name,
                DisplayName = addTagRequest.DisplayName
            };

            await tagRepository.AddSync(tag);
            return RedirectToAction("List");
        }
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var tags=await tagRepository.GetAllAsync();
            return View(tags);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var tag = await tagRepository.GetAsync(id);
            if (tag != null)
            {
                var editTagRequest = new EditTagRequest
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    DisplayName = tag.DisplayName
                };
                return View(editTagRequest);
            }

            return View(null);
        }
        [HttpPost]
        public async Task<IActionResult>Edit(EditTagRequest editTagRequest)
        {
            var tag = new Tag
            {
                Id = editTagRequest.Id,
                Name = editTagRequest.Name,
                DisplayName = editTagRequest.DisplayName
            };
            var updatedTag= await tagRepository.UpdateSync(tag);
            if (updatedTag != null)
            {
                return RedirectToAction("List");
            }
            else
            {
                //show error message
                return RedirectToAction("Edit", new { id = editTagRequest.Id });
            }
        }
        public async Task<IActionResult> Delete(EditTagRequest editTagRequest)
        {
            var deletedTag=await tagRepository.DeleteSync(editTagRequest.Id);
            if (deletedTag != null)
            {
                return RedirectToAction("List");
            }
            return RedirectToAction("Edit", new { id = editTagRequest.Id });
        }
    }
}
