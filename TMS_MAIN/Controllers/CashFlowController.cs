using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.Linq;
using System.Text;
using TMS_MAIN.Models;
using TMS_MAIN.Services;

namespace TMS_MAIN.Controllers
{
    public class CashFlowController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CashFlowController(ITransactionService transactionService, IHttpContextAccessor httpContextAccessor)
        {
            _transactionService = transactionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult CFIndex()
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account"); // Redirect if not logged in
            }
            var cashflows = _transactionService.GetCashFlowsByUserId(loggedInUserId.Value);
            return View(cashflows);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.TransactionTypes = new SelectList(Enum.GetValues(typeof(TransactionType)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CFViewModel viewModel)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.TransactionTypes = new SelectList(Enum.GetNames(typeof(TransactionType)));
                return View(viewModel);
            }

            var cashFlow = new CashFlow
            {
                TransactionDate = viewModel.TransactionDate,
                AccountId = viewModel.AccountId,
                Amount = viewModel.Amount,
                Description = viewModel.Description,
                TransactionType = Enum.Parse<TransactionType>(viewModel.TransactionType),
                UserId = loggedInUserId.Value
            };

            if (!_transactionService.RecordTransaction(cashFlow, out string errorMessage))
            {
                ModelState.AddModelError(string.Empty, errorMessage);
                ViewBag.TransactionTypes = new SelectList(Enum.GetNames(typeof(TransactionType)));
                return View(viewModel);
            }

            return RedirectToAction("CFIndex");
        }

        public IActionResult GetCashFlowReport(DateTime? startDate, DateTime? endDate, int? aid)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account"); // Redirect if not logged in
            }

            if (startDate == null || endDate == null)
            {
                startDate = DateTime.Today.AddMonths(-1); // Default: Last month
                endDate = DateTime.Today;
            }
            var cashFlows = _transactionService.GetCashFlows(startDate.Value, endDate.Value)
                                       .Where(cf => cf.UserId == loggedInUserId.Value)
                                       .OrderByDescending(t => t.TransactionDate)
                                       .ToList();
            if (aid.HasValue)
            {
                cashFlows = cashFlows.Where(cf => cf.AccountId == aid.Value).ToList();
            }
            var totalInflow = cashFlows.Where(cf => cf.TransactionType == TransactionType.Inflow).Sum(cf => cf.Amount);
            var totalOutflow = cashFlows.Where(cf => cf.TransactionType == TransactionType.Outflow).Sum(cf => cf.Amount);
            var netCashFlow = totalInflow - totalOutflow;
            var report = new CashFlowReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value,
                GeneratedDate = DateTime.Now,
                AccountId = aid ?? 0, // Use 0 or another default if aid is null
                //AccountName = accname ?? "N/A",
                TotalInflow = totalInflow,
                TotalOutflow = totalOutflow,
                NetCashFlow = netCashFlow,
                CashFlows = cashFlows
            };
            return View(report);


        }

        //Csv
        public IActionResult DownloadCsv(DateTime startDate, DateTime endDate, int aid, int userId)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account"); // Redirect if not logged in
            }
            var model = _transactionService.GetReport(startDate, endDate, aid, userId); // method in service

            var csv = new StringBuilder();
            csv.AppendLine("Transaction Date,Transaction Type,Amount,Description");

            foreach (var item in model.CashFlows)
            {
                csv.AppendLine($"{item.TransactionDate},{item.TransactionType},{item.Amount},{item.Description}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"CashFlowReport_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}