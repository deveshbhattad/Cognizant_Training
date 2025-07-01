
using Microsoft.AspNetCore.Mvc;
using TMS_MAIN.Services;
using TMS_MAIN.Models;
using TMS_MAIN.ViewModels;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using System;
using Microsoft.AspNetCore.Http; // For session handling.
using Microsoft.AspNetCore.Mvc.Rendering; // For UI elements like dropdowns.

namespace TMS_MAIN.Controllers
{
    public class InvestmentController : Controller
    {
        private readonly IInvestmentService _investmentService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConverter _converter;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InvestmentController(
            IInvestmentService investmentService,
            IViewRenderService viewRenderService,
            IConverter converter,
            IHttpContextAccessor httpContextAccessor)
        {
            _investmentService = investmentService;
            _viewRenderService = viewRenderService;
            _converter = converter;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            // Displays all investments for the logged-in user.
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");

            if (!loggedInUserId.HasValue) // Gets the user ID from session.
            {
                return RedirectToAction("Login", "Account");
            }
            // Redirects to login if not logged in.
            var investments = _investmentService.GetInvestmentsByUserId(loggedInUserId.Value);
            return View(investments);
        }
        [HttpGet]
        public IActionResult AddInvestment()
        {
            ViewBag.InvestmentTypes = new SelectList(Enum.GetValues(typeof(InvestmentType)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddInvestment(IViewModel model)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }


            if (ModelState.IsValid)
            {
                InvestmentType parsedType;
                if (!Enum.TryParse(model.InvestmentType, out parsedType))
                {
                    ModelState.AddModelError("InvestmentType", "Invalid investment type selected.");
                    ViewBag.InvestmentTypes = new SelectList(Enum.GetValues(typeof(InvestmentType)));
                    return View(model);
                }

                var investment = new Investment
                {
                    InvestmentType = parsedType,
                    AmountInvested = model.AmountInvested,
                    CurrentValue = model.CurrentValue,
                    PurchaseDate = model.PurchaseDate,
                    MaturityDate = model.MaturityDate,
                    UserId = loggedInUserId.Value
                };

                _investmentService.AddInvestment(investment);
                return RedirectToAction("Index");
            }
            ViewBag.InvestmentTypes = new SelectList(Enum.GetValues(typeof(InvestmentType)));
            return View(model);
        }
        public IActionResult Details(int id)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var investment = _investmentService.GetInvestmentById(id);
            if (investment == null)
            {
                return NotFound();
            }
            return View(investment);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var investment = _investmentService.GetInvestmentById(id);
            if (investment == null || investment.UserId != loggedInUserId.Value)
            {
                return NotFound();
            }
            return View(investment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Investment models)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id != models.InvestmentId)
            {
                return NotFound();
            }

            /* if (ModelState.IsValid)     // first code
             {
                 _investmentService.UpdateInvestment(model);
                 return RedirectToAction("Index");
             }

             return View(model);*/

            var existingInvestment = _investmentService.GetInvestmentById(id);
            if (existingInvestment == null || existingInvestment.UserId != loggedInUserId.Value)
            {
                return NotFound(); // Or UnauthorizedResult
            }
            models.UserId = loggedInUserId.Value; // Ensure UserId is not changed maliciously
            _investmentService.UpdateInvestment(models);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            var investment = _investmentService.GetInvestmentById(id);
            if (investment == null)
            {
                return NotFound();
            }
            return View(investment);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            _investmentService.DeleteInvestment(id);
            return RedirectToAction(nameof(Index));
        }
        /*public IActionResult PortfolioSummary()
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            var summary = _investmentService.GetPortfolioSummary();
            return View(summary);
        }
        [HttpGet]
        public async Task<IActionResult> PortfolioSummaryPdf()
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            var model = _investmentService.GetPortfolioSummary();
            var htmlContent = await _viewRenderService.RenderToStringAsync("Investment/PortfolioSummary", model);
            var pdfDoc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    DocumentTitle = "Portfolio Summary",
                    Margins = new MarginSettings { Top = 10, Bottom = 10 }
                },
                Objects = { new ObjectSettings{
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }}}
            };
            var pdf = _converter.Convert(pdfDoc); return File(pdf, "application/pdf", "PortfolioSummary.pdf");
        }*/

        public IActionResult PortfolioSummary()
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var summary = _investmentService.GetPortfolioSummary(loggedInUserId.Value); // Pass userId
            return View(summary);
        }

        [HttpGet]
        public async Task<IActionResult> PortfolioSummaryPdf()
        {
            int? loggedInUserId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!loggedInUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = _investmentService.GetPortfolioSummary(loggedInUserId.Value); // Pass userId
            var htmlContent = await _viewRenderService.RenderToStringAsync("Investment/PortfolioSummary", model);

            var pdfDoc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    DocumentTitle = "Portfolio Summary",
                    Margins = new MarginSettings { Top = 10, Bottom = 10 }
                },
                Objects = {
                new ObjectSettings {
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            var pdf = _converter.Convert(pdfDoc);
            return File(pdf, "application/pdf", "PortfolioSummary.pdf");
        }
    }
}
