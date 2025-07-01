// AdminReportsViewModel.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMS_MAIN.Models;

namespace TMS_MAIN.ViewModels
{
    public class AdminReportsViewModel
    {
        public int SelectedUserId { get; set; }
        public string SelectedUserName { get; set; }
        public List<Report> Reports { get; set; } = new List<Report>();
        public AdminReportFilterViewModel FilterModel { get; set; }

        public List<SelectListItem> Users { get; set; }

        public PortfolioSummaryViewModel PortfolioSummary { get; set; }


    }

}