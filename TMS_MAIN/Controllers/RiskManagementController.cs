using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TMS_MAIN.Models;
using TMS_MAIN.Services;

namespace TMS_MAIN.Controllers
{
    public class RiskManagementController : Controller
    {
        private readonly IRiskAssessmentService _riskAssessmentService;

        public RiskManagementController(IRiskAssessmentService riskAssessmentService)
        {
            _riskAssessmentService = riskAssessmentService;
        }

        // Helper to get the current user's ID from session
        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        // Helper to check session and redirect if not logged in
        private IActionResult CheckSession()
        {
            if (GetCurrentUserId() == null)
                return RedirectToAction("Login", "Account");
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> RMIndex()
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var currentUserId = GetCurrentUserId().Value;
            var risks = await _riskAssessmentService.GetAllAsync(currentUserId);
            return View(risks);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var newRisk = new Risk
            {
                AssessmentDate = DateTime.Today,
                RiskScore = 0,
                RiskLevel = "N/A",
                Amount = 0m,
                Impact = 1,
                Probability = 1
            };
            return View(newRisk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RiskId,RiskType,TransactionReference,Amount,Impact,Probability,AssessmentDate")] Risk risk)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var currentUserId = GetCurrentUserId().Value;
            risk.UserId = currentUserId;

            if (!ModelState.IsValid)
            {
                try
                {
                    // Call the service to add the risk.
                    // The AddAsync method in the service will internally call SuggestRiskValues(risk)
                    // to calculate RiskScore and RiskLevel based on the provided Amount, Impact, and Probability
                    // before saving the Risk entity (including the calculated RiskScore) to the database.
                    await _riskAssessmentService.AddAsync(risk);
                    TempData["SuccessMessage"] = "Risk Assessment added successfully!"; // Message for the next page
                    // Redirect to the RiskScore page to display the details of the newly created risk.
                    return RedirectToAction(nameof(RMIndex));
                }
                catch (InvalidOperationException ex) // Catch specific application-level errors (e.g., duplicate RiskId)
                {
                    ModelState.AddModelError("RiskId", ex.Message); // Add error to specific field
                }
                catch (Exception ex) // Catch any other unexpected errors during the addition process
                {
                    ModelState.AddModelError("", $"An error occurred while adding the risk assessment: {ex.Message}"); // General error
                }
            }
            // If ModelState is not valid (due to validation errors or caught exceptions),
            // re-render the Create view. This preserves user input and displays error messages.
            // Re-calculate RiskScore/RiskLevel for display on the form using the current (potentially invalid) inputs.
            risk = _riskAssessmentService.SuggestRiskValues(risk);
            return View(risk);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId().Value;
            var risk = await _riskAssessmentService.GetByIdAsync(id, currentUserId);
            if (risk == null)
            {
                return NotFound();
            }

            risk = _riskAssessmentService.SuggestRiskValues(risk);
            return View(risk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("RiskId,RiskType,TransactionReference,Amount,Impact,Probability,AssessmentDate,UserId")] Risk risk)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            if (id != risk.RiskId)
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId().Value;
            risk.UserId = currentUserId;

            if (!ModelState.IsValid)
            {
                try
                {
                    await _riskAssessmentService.UpdateAsync(risk, currentUserId);
                    TempData["SuccessMessage"] = "Risk Assessment updated successfully!";
                    return RedirectToAction(nameof(RMIndex));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await RiskAssessmentExists(risk.RiskId, currentUserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    return Forbid();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred while updating the risk assessment: {ex.Message}");
                }
            }
            risk = _riskAssessmentService.SuggestRiskValues(risk);
            return View(risk);
        }

        private async Task<bool> RiskAssessmentExists(string id, int userId)
        {
            return await _riskAssessmentService.GetByIdAsync(id, userId) != null;
        }

        public async Task<IActionResult> Delete(string id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId().Value;
            var risk = await _riskAssessmentService.GetByIdAsync(id, currentUserId);
            if (risk == null)
            {
                return NotFound();
            }
            risk = _riskAssessmentService.SuggestRiskValues(risk);
            return View(risk);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var currentUserId = GetCurrentUserId().Value;
            try
            {
                await _riskAssessmentService.DeleteAsync(id, currentUserId);
                TempData["SuccessMessage"] = "Risk Assessment deleted successfully!";
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred during deletion: {ex.Message}");
            }
            return RedirectToAction(nameof(RMIndex));
        }

        public async Task<IActionResult> Details(string id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = GetCurrentUserId().Value;
            var risk = await _riskAssessmentService.GetByIdAsync(id, currentUserId);
            if (risk == null)
            {
                return NotFound();
            }
            risk = _riskAssessmentService.SuggestRiskValues(risk);
            return View(risk);
        }

        public async Task<IActionResult> AnalyzeRisk(
            string? riskId,
            string? riskType,
            string? transactionReference,
            DateTime? startDate,
            DateTime? endDate)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var currentUserId = GetCurrentUserId().Value;

            var viewModel = new AnalyzeFinancialRisksViewModel
            {
                RiskId = riskId,
                RiskType = riskType,
                TransactionReference = transactionReference,
                StartDate = startDate,
                EndDate = endDate
            };

            viewModel.Risks = await _riskAssessmentService.GetFilteredAsync(
                riskId, riskType, transactionReference, startDate, endDate, currentUserId);

            foreach (var risk in viewModel.Risks)
            {
                _riskAssessmentService.SuggestRiskValues(risk);
            }

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetRiskManagementReport(DateTime? startDate, DateTime? endDate)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var currentUserId = GetCurrentUserId().Value;
            var reportViewModel = await _riskAssessmentService.GetReportAsync(startDate, endDate, currentUserId);
            return View(reportViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> RiskScore(string id)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var viewModel = new RiskScoreViewModel();

            if (!string.IsNullOrEmpty(id))
            {
                var currentUserId = GetCurrentUserId().Value;
                var risk = await _riskAssessmentService.GetByIdAsync(id, currentUserId);
                if (risk != null)
                {
                    viewModel.InputRiskId = id;
                    viewModel.FoundRiskId = risk.RiskId;
                    viewModel.FoundRiskType = risk.RiskType;
                    viewModel.FoundTransactionReference = risk.TransactionReference;
                    viewModel.FoundRiskScore = risk.RiskScore;
                    viewModel.FoundAssessmentDate = risk.AssessmentDate;
                    risk = _riskAssessmentService.SuggestRiskValues(risk);
                    viewModel.FoundRiskLevel = risk.RiskLevel;

                    viewModel.Message = $"Details for Risk ID '{risk.RiskId}' loaded successfully.";
                }
                else
                {
                    viewModel.InputRiskId = id;
                    viewModel.Message = $"Risk with ID '{id}' not found or you do not have permission to view it.";
                }
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RiskScore(RiskScoreViewModel model)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            if (ModelState.IsValid)
            {
                var currentUserId = GetCurrentUserId().Value;
                var risk = await _riskAssessmentService.GetByIdAsync(model.InputRiskId, currentUserId);
                if (risk != null)
                {
                    model.FoundRiskId = risk.RiskId;
                    model.FoundRiskType = risk.RiskType;
                    model.FoundTransactionReference = risk.TransactionReference;
                    model.FoundRiskScore = risk.RiskScore;
                    model.FoundAssessmentDate = risk.AssessmentDate;
                    risk = _riskAssessmentService.SuggestRiskValues(risk);
                    model.FoundRiskLevel = risk.RiskLevel;

                    model.Message = $"Details for Risk ID '{risk.RiskId}' found successfully.";
                }
                else
                {
                    model.Message = $"Risk with ID '{model.InputRiskId}' not found or you do not have permission to view it.";
                    model.FoundRiskId = null;
                    model.FoundRiskType = null;
                    model.FoundTransactionReference = null;
                    model.FoundRiskScore = null;
                    model.FoundAssessmentDate = null;
                    model.FoundRiskLevel = null;
                }
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult SuggestValues([FromBody] Risk risk)
        {
            var sessionCheck = CheckSession();
            if (sessionCheck != null) return sessionCheck;

            var suggestedRisk = _riskAssessmentService.SuggestRiskValues(risk);
            return Json(suggestedRisk);
        }
    }
}
