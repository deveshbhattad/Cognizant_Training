using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMS_MAIN.Models;
using TMS_MAIN.Services;
using TMS_MAIN.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;

namespace TMS_MAIN.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IBankAccountService _bankAccountService;
        private readonly ITransactionService _transactionService;
        private readonly IInvestmentService _investmentService;
        private readonly IRiskAssessmentService _riskAssessmentService;
        private readonly ILogger<ReportsController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor; // Now injected here

        public ReportsController(
            IReportService reportService,
            IBankAccountService bankAccountService,
            ITransactionService transactionService,
            IInvestmentService investmentService,
            IRiskAssessmentService riskAssessmentService,
            ILogger<ReportsController> logger,
            IHttpContextAccessor httpContextAccessor) // Added httpContextAccessor
        {
            _reportService = reportService;
            _bankAccountService = bankAccountService;
            _transactionService = transactionService;
            _investmentService = investmentService;
            _riskAssessmentService = riskAssessmentService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor; // Initialized here
        }

        // ... rest of your code

        // GET: Reports
        public IActionResult Index()
        {
            try
            {
                var userId = CurrentUserId;
                var reports = _reportService.GetReportsByUserId(userId);
                return View(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports index");
                TempData["ErrorMessage"] = "Error loading reports. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Reports/Generate
       
        public IActionResult Generate()
        {
            try
            {
                var userId = CurrentUserId;
                var bankAccounts = _bankAccountService.GetBankAccountsByUserId(userId);

                var model = new ReportViewModel
                {
                    StartDate = DateTime.Today.AddMonths(-1),
                    EndDate = DateTime.Today,
                    BankAccounts = bankAccounts.Select(b => new SelectListItem
                    {
                        Value = b.AccountId.ToString(),
                        Text = $"{b.BankName} - {b.AccountNumber} ({b.AccountType})"
                    }).ToList()
                };

                ViewBag.BankAccounts = model.BankAccounts;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report generation form");
                TempData["ErrorMessage"] = "Error loading report form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Reports/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Generate(ReportViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload bank accounts if validation fails
                var userId = CurrentUserId;
                model.BankAccounts = _bankAccountService.GetBankAccountsByUserId(userId)
                    
                    .Select(b => new SelectListItem
                    {
                        Value = b.AccountId.ToString(),
                        Text = $"{b.BankName} - {b.AccountNumber} ({b.AccountType})"
                    }).ToList();
                return View(model);
            }

            try
            {
                var userId = CurrentUserId;

                if (model.Module == "BankAccount" && model.AccountId == 0)
                {
                    ModelState.AddModelError("AccountId", "Please select a bank account");

                    // Reload bank accounts
                    model.BankAccounts = _bankAccountService.GetBankAccountsByUserId(userId)
                        .Select(b => new SelectListItem
                        {
                            Value = b.AccountId.ToString(),
                            Text = $"{b.BankName} - {b.AccountNumber} ({b.AccountType})"
                        }).ToList();

                    return View(model);
                }


                var report = new Report
                {
                    ReportName = model.ReportName,
                    Module = model.Module,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    UserId = userId,
                    Parameters = JsonConvert.SerializeObject(new { AccountId = model.AccountId }),
                    Status = "Generated",
                    GeneratedDate = DateTime.Now
                };

                var generatedReport = _reportService.GenerateReport(report);

                return RedirectToAction(nameof(ViewReport), new { id = generatedReport.ReportId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                ModelState.AddModelError("", $"Error generating report: {ex.Message}");

                // Reload bank accounts on error
                var userId = CurrentUserId;
                model.BankAccounts = _bankAccountService.GetBankAccountsByUserId(userId)
                    .Select(b => new SelectListItem
                    {
                        Value = b.AccountId.ToString(),
                        Text = $"{b.BankName} - {b.AccountNumber} ({b.AccountType})"
                    }).ToList();

                return View(model);
            }
        }

        // GET: Reports/ViewReport/5
        public IActionResult ViewReport(int id)
        {
            try
            {
                var userId = CurrentUserId;
                var report = _reportService.GetReportById(id, userId);

                if (report == null)
                {
                    TempData["ErrorMessage"] = "Report not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (report.Status == "Generated")
                {
                    _reportService.UpdateReportStatus(id, "Viewed");
                    report.Status = "Viewed";
                }

                object reportData;

                switch (report.Module)
                {
                    case "CashFlow":
                        reportData = _transactionService.GetReport(
                            report.StartDate,
                            report.EndDate,
                            (int?)(JsonConvert.DeserializeObject<dynamic>(report.Parameters ?? "{}")?.AccountId) ?? 0,
                            report.UserId
                        ) ?? new CashFlowReportViewModel();
                        break;
                    case "Investment":
                        reportData = _investmentService.GetPortfolioSummary(report.UserId)
                            ?? new PortfolioSummaryViewModel();
                        break;
                    case "Risk":
                        reportData = _riskAssessmentService.GetReportAsync(
                            report.StartDate,
                            report.EndDate,
                            report.UserId).Result ?? new RiskReportViewModel();
                        break;
                    case "BankAccount":
                        try
                        {
                            var parameters = JsonConvert.DeserializeObject<dynamic>(report.Parameters ?? "{}");
                            var accountId = (int?)parameters?.AccountId ?? 0;

                            _logger.LogInformation($"BankAccount Report - AccountId: {accountId}, UserId: {report.UserId}");

                            var bankReport = _bankAccountService.GetBankAccountReport(
                                report.StartDate,
                                report.EndDate,
                                accountId,
                                report.UserId);

                            if (!bankReport.HasData)
                            {
                                _logger.LogWarning($"No data found for bank account report - AccountId: {accountId}");

                                TempData["WarningMessage"] = "No data found for the selected bank account report.";

                            }

                            reportData = bankReport;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing bank account report");
                            reportData = new BankAccountReportViewModel();
                            TempData["ErrorMessage"] = "Error processing bank account report. Please try again.";
                        }
                        break;
                    default:
                        reportData = null;
                        break;
                }


                var compliances = _reportService.GetCompliancesByReportId(id);

                return View(new ReportDisplayViewModel
                {
                    Report = report,
                    ReportData = reportData,
                    Compliances = compliances
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing report with ID {id}");
                TempData["ErrorMessage"] = "Error viewing report. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Update the PrintReport action:
        public IActionResult PrintReport(int id)
        {
            try
            {
                var userId = CurrentUserId;
                var report = _reportService.GetReportById(id, userId);

                if (report == null)
                {
                    TempData["ErrorMessage"] = "Report not found.";
                    return RedirectToAction(nameof(Index));
                }

                object reportData;

                switch (report.Module)
                {
                    case "CashFlow":
                        reportData = _transactionService.GetReport(
                            report.StartDate,
                            report.EndDate,
                            (int?)(JsonConvert.DeserializeObject<dynamic>(report.Parameters ?? "{}")?.AccountId) ?? 0,
                            report.UserId
                        ) ?? new CashFlowReportViewModel();
                        break;
                    case "Investment":
                        reportData = _investmentService.GetPortfolioSummary(report.UserId)
                            ?? new PortfolioSummaryViewModel();
                        break;
                    case "Risk":
                        reportData = _riskAssessmentService.GetReportAsync(
                            report.StartDate,
                            report.EndDate,
                            report.UserId).Result ?? new RiskReportViewModel();
                        break;
                    case "BankAccount":
                        try
                        {
                            var parameters = JsonConvert.DeserializeObject<dynamic>(report.Parameters ?? "{}");
                            var accountId = (int?)parameters?.AccountId ?? 0;

                            _logger.LogInformation($"BankAccount Report - AccountId: {accountId}, UserId: {report.UserId}");

                            var bankReport = _bankAccountService.GetBankAccountReport(
                                report.StartDate,
                                report.EndDate,
                                accountId,
                                report.UserId);

                            if (!bankReport.HasData)
                            {
                                _logger.LogWarning($"No data found for bank account report - AccountId: {accountId}");

                                TempData["WarningMessage"] = "No data found for the selected bank account report.";

                            }

                            reportData = bankReport;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing bank account report");
                            reportData = new BankAccountReportViewModel();
                            TempData["ErrorMessage"] = "Error processing bank account report. Please try again.";
                        }
                        break;
                    default:
                        reportData = null;
                        break;
                }

                var compliances = _reportService.GetCompliancesByReportId(id);

                return View(new ReportDisplayViewModel
                {
                    Report = report,
                    ReportData = reportData,
                    Compliances = compliances
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error printing report with ID {id}");
                TempData["ErrorMessage"] = "Error generating print version.";
                return RedirectToAction(nameof(ViewReport), new { id });
            }
        }


        // POST: Reports/SubmitCompliance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitCompliance(int reportId, string notes)
        {
            try
            {
                var userId = CurrentUserId;

                var compliance = new Compliance
                {
                    ReportId = reportId,
                    UserId = userId,
                    Notes = notes,
                    Status = "Submitted",
                    SubmissionDate = DateTime.Now
                };

                _reportService.SubmitCompliance(compliance);

                TempData["SuccessMessage"] = "Compliance submitted successfully.";
                return RedirectToAction(nameof(ViewReport), new { id = reportId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error submitting compliance for report {reportId}");
                TempData["ErrorMessage"] = "Error submitting compliance. Please try again.";
                return RedirectToAction(nameof(ViewReport), new { id = reportId });
            }
        }



        //// GET: Reports/PrintReport/5
        //public IActionResult PrintReport(int id)
        //{
        //    try
        //    {
        //        var userId = CurrentUserId;
        //        var report = _reportService.GetReportById(id, userId);

        //        if (report == null)
        //        {
        //            TempData["ErrorMessage"] = "Report not found.";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        object reportData = report.Module switch
        //        {
        //            "CashFlow" => _transactionService.GetReport(
        //                report.StartDate,
        //                report.EndDate,
        //                JsonConvert.DeserializeObject<dynamic>(report.Parameters ?? "{}")?.AccountId ?? 0,
        //                report.UserId) ?? new CashFlowReportViewModel(),
        //            "Investment" => _investmentService.GetPortfolioSummary(report.UserId)
        //                ?? new PortfolioSummaryViewModel(),
        //            "Risk" => _riskAssessmentService.GetReportAsync(
        //                report.StartDate,
        //                report.EndDate,
        //                report.UserId).Result ?? new RiskReportViewModel(),
        //            _ => null
        //        };

        //        var compliances = _reportService.GetCompliancesByReportId(id);

        //        var viewModel = new ReportDisplayViewModel
        //        {
        //            Report = report,
        //            ReportData = reportData,
        //            Compliances = compliances
        //        };

        //        // Set a layout that's optimized for printing
        //        ViewBag.UsePrintLayout = true;

        //        return View(viewModel);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error printing report with ID {id}");
        //        TempData["ErrorMessage"] = "Error generating print version.";
        //        return RedirectToAction(nameof(ViewReport), new { id });
        //    }
        //}




        // GET: Reports/Delete/5
        public IActionResult Delete(int id)
        {
            try
            {
                var userId = CurrentUserId;
                var report = _reportService.GetReportById(id, userId);

                if (report == null)
                {
                    TempData["ErrorMessage"] = "Report not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading delete confirmation for report {id}");
                TempData["ErrorMessage"] = "Error loading delete confirmation.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Reports/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int ReportId)  // Note the parameter name matches the form field
        {
            try
            {
                var userId = CurrentUserId;
                var success = _reportService.DeleteReport(ReportId, userId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Report deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Report not found or you don't have permission to delete it.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting report {ReportId}");
                TempData["ErrorMessage"] = "Error deleting report. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }


        private int CurrentUserId
        {
            get
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    throw new InvalidOperationException("User is not logged in.");
                }
                return loggedInUserId.Value;
            }
        }

    }
}