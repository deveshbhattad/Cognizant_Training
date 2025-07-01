

// ViewModels/ReportDisplayViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using TMS_MAIN.Models;

namespace TMS_MAIN.ViewModels
{

    public class AdminReportFilterViewModel
    {
        [Required(ErrorMessage = "Please select a user")]
        [Display(Name = "Select User")]
        public int SelectedUserId { get; set; }

        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();
        public AdminReportFilterViewModel FilterModel { get; internal set; }
    }
    public class ReportDisplayViewModel
    {
        public Report Report { get; set; }
        public object ReportData { get; set; }
        public IEnumerable<Compliance> Compliances { get; set; }
        public string ReportType { get; internal set; }
        public bool IsPrintView { get; internal set; }

        public bool IsAdminView { get; set; } = false;
    }
}