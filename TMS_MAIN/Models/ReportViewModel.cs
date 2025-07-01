// ViewModels/ReportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMS_MAIN.Models;

namespace TMS_MAIN.Models
{
    public class ReportViewModel
    {
        [Required(ErrorMessage = "Report name is required")]
        [StringLength(100, ErrorMessage = "Report name cannot exceed 100 characters")]
        public string ReportName { get; set; }

        [Required(ErrorMessage = "Module is required")]
        public string Module { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int? AccountId { get; set; }

        public List<SelectListItem> BankAccounts { get; set; } = new List<SelectListItem>();
    }
}