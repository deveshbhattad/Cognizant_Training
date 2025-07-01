using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TMS_MAIN.Data;
using TMS_MAIN.Services;
using TMS_MAIN.ViewModels;

namespace TMS_MAIN.Controllers
{
    public class AdminController : Controller
    {
        private readonly TreasuryManagementSystemContext _context;
        private readonly IReportService _reportService;
        private readonly ILogger<AdminController> _logger;


        // Use a single constructor and inject all dependencies
        public AdminController(
            TreasuryManagementSystemContext context,
            IReportService reportService,
            ILogger<AdminController> logger)
        {
            _context = context;
            _reportService = reportService;
            _logger = logger;
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        public async Task<IActionResult> AdminReports()
        {
            var users = await _context.Users
                .Where(u => u.IsAdmin == false)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var model = new AdminReportsViewModel
            {
                FilterModel = new TMS_MAIN.ViewModels.AdminReportFilterViewModel
                {
                    Users = users.Select(u => new SelectListItem
                    {
                        Value = u.UserId.ToString(),
                        Text = $"{u.FullName} (ID: {u.UserId})"
                    }).ToList()
                }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AdminReports(AdminReportFilterViewModel filterModel)
        {
            var users = await _context.Users
                .Where(u => u.IsAdmin == false)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var model = new AdminReportsViewModel
            {
                FilterModel = new TMS_MAIN.ViewModels.AdminReportFilterViewModel
                {
                    Users = users.Select(u => new SelectListItem
                    {
                        Value = u.UserId.ToString(),
                        Text = $"{u.FullName} (ID: {u.UserId})"
                    }).ToList(),
                    SelectedUserId = filterModel.SelectedUserId
                }
            };

            if (filterModel.SelectedUserId == 0)
            {
                ModelState.AddModelError("FilterModel.SelectedUserId", "Please select a user");
                return View(model);
            }

            model.SelectedUserId = filterModel.SelectedUserId;
            model.SelectedUserName = users.FirstOrDefault(u => u.UserId == filterModel.SelectedUserId)?.FullName;
            model.Reports = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.Compliances)
                .ThenInclude(c => c.User)
                .Where(r => r.UserId == filterModel.SelectedUserId)
                .OrderByDescending(r => r.GeneratedDate)
                .ToListAsync();

            return View(model);
        }

        public async Task<IActionResult> ViewAdminReport(int id)
        {
            var report = await _context.Reports
                .Include(r => r.User)
                .Include(r => r.Compliances)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }
        public async Task<IActionResult> PrintReport(int id)
        {
            try
            {
                var report = await _context.Reports
                    .Include(r => r.User)
                    .Include(r => r.Compliances)
                    .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(r => r.ReportId == id);

                if (report == null)
                {
                    TempData["ErrorMessage"] = "Report not found.";
                    return RedirectToAction("AdminReports");
                }

                // Get the report data
                var reportData = _reportService.GenerateReportData(report);

                // Debug log the data
                _logger.LogInformation("Print Report Data: {Data}",
                    JsonConvert.SerializeObject(reportData, Formatting.Indented));

                // Create a simplified print layout
                var printModel = new ReportDisplayViewModel
                {
                    Report = report,
                    ReportData = reportData,
                    Compliances = report.Compliances.ToList(),
                    IsPrintView = true
                };

                return View("~/Views/Reports/PrintReport.cshtml", printModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error generating print version.";
                return RedirectToAction("ViewReport", new { id });
            }
        }




    }
}
