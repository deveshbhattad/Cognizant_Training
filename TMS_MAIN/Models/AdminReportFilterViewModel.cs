using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class AdminReportFilterViewModel
{
    [Required(ErrorMessage = "Please select a user")]
    [Display(Name = "Select User")]
    public int SelectedUserId { get; set; }

    public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
}