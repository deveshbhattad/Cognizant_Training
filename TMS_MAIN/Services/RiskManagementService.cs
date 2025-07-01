// Services/RiskAssessmentService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TMS_MAIN.Data;
using TMS_MAIN.Models;

namespace TMS_MAIN.Services
{
    // Corrected class name to match IRiskAssessmentService
    public class RiskManagementService : IRiskAssessmentService
    {
        private readonly TreasuryManagementSystemContext _context;

        public RiskManagementService(TreasuryManagementSystemContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all risk assessments for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose risks to retrieve.</param>
        /// <returns>A list of risks belonging to the specified user, ordered by assessment date.</returns>
        public async Task<IList<Risk>> GetAllAsync(int userId)
        {
            return await _context.RiskAssessments
                                 .Where(r => r.UserId == userId) // Filter by UserId
                                 .OrderByDescending(r => r.AssessmentDate)
                                 .ToListAsync(); 
        }

        /// <summary>
        /// Retrieves a specific risk assessment by its ID, ensuring it belongs to the specified user.
        /// </summary>
        /// <param name="id">The unique ID of the risk assessment.</param>
        /// <param name="userId">The ID of the user who owns the risk.</param>
        /// <returns>The Risk object if found and owned by the user; otherwise, null.</returns>
        public async Task<Risk?> GetByIdAsync(string id, int userId)
        {
            return await _context.RiskAssessments
                                 .FirstOrDefaultAsync(r => r.RiskId == id && r.UserId == userId); // Filter by both RiskId and UserId
        }
        /// <summary>
        /// Adds a new risk assessment to the database.
        /// </summary>
        /// <param name="risk">The Risk object to add. The UserId is expected to be pre-set by the caller.</param>
        /// <exception cref="InvalidOperationException">Thrown if a risk with the same ID already exists or if UserId is not set.</exception>
        public async Task AddAsync(Risk risk)
        {
            // A basic check, though the controller should populate UserId
            if (risk.UserId == 0)
            {
                throw new InvalidOperationException("User ID must be assigned to the risk before adding.");
            }

            if (await _context.RiskAssessments.AnyAsync(r => r.RiskId == risk.RiskId))
            {
                throw new InvalidOperationException($"Risk Assessment with ID '{risk.RiskId}' already exists.");
            }
            CalculateRiskScoreAndLevel(risk); // Calculate score and level before adding
            _context.RiskAssessments.Add(risk);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing risk assessment, performing an ownership check.
        /// </summary>
        /// <param name="risk">The Risk object with updated values.</param>
        /// <param name="userId">The ID of the user attempting to update the risk.</param>
        /// <exception cref="InvalidOperationException">Thrown if the risk is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user does not own the risk.</exception>
        public async Task UpdateAsync(Risk risk, int userId)
        {
            // Fetch the existing risk to verify ownership before allowing the update
            // Using AsNoTracking to avoid tracking issues if 'risk' is already tracked
            var existingRisk = await _context.RiskAssessments.AsNoTracking().FirstOrDefaultAsync(r => r.RiskId == risk.RiskId);

            if (existingRisk == null)
            {
                throw new InvalidOperationException($"Risk Assessment with ID '{risk.RiskId}' not found.");
            }

            // **Crucial Security Check**: Verify that the risk belongs to the user trying to update it.
            if (existingRisk.UserId != userId)
            {
                throw new UnauthorizedAccessException($"User is not authorized to update risk with ID '{risk.RiskId}'.");
            }

            // Ensure the UserId from the provided 'risk' object is not accidentally changed.
            // It should always be the original owner's ID.
            risk.UserId = existingRisk.UserId;

            CalculateRiskScoreAndLevel(risk); // Recalculate score and level before updating
            _context.Entry(risk).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a risk assessment, performing an ownership check.
        /// </summary>
        /// <param name="id">The ID of the risk to delete.</param>
        /// <param name="userId">The ID of the user attempting to delete the risk.</param>
        /// <exception cref="InvalidOperationException">Thrown if the risk is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user does not own the risk.</exception>
        public async Task DeleteAsync(string id, int userId)
        {
            var risk = await _context.RiskAssessments.FindAsync(id);
            if (risk == null)
            {
                throw new InvalidOperationException($"Risk Assessment with ID '{id}' not found.");
            }

            // **Crucial Security Check**: Verify that the risk belongs to the user trying to delete it.
            if (risk.UserId != userId)
            {
                throw new UnauthorizedAccessException($"User is not authorized to delete risk with ID '{id}'.");
            }

            _context.RiskAssessments.Remove(risk);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a filtered list of risk assessments for a specific user.
        /// </summary>
        /// <param name="riskId">Optional: Filter by risk ID (contains).</param>
        /// <param name="riskType">Optional: Filter by risk type (contains).</param>
        /// <param name="transactionReference">Optional: Filter by transaction reference (contains).</param>
        /// <param name="startDate">Optional: Filter by assessment date from this date.</param>
        /// <param name="endDate">Optional: Filter by assessment date up to this date.</param>
        /// <param name="userId">The ID of the user whose risks to filter.</param>
        /// <returns>A filtered list of risks belonging to the specified user.</returns>
        public async Task<IList<Risk>> GetFilteredAsync(
            string? riskId,
            string? riskType,
            string? transactionReference,
            DateTime? startDate,
            DateTime? endDate,
            int userId) // Added userId parameter
        {
            IQueryable<Risk> risksQuery = _context.RiskAssessments
                                                 .Where(r => r.UserId == userId); // Apply user filter first

            if (!string.IsNullOrEmpty(riskId))
            {
                risksQuery = risksQuery.Where(r => r.RiskId.Contains(riskId));
            }

            if (!string.IsNullOrEmpty(riskType))
            {
                risksQuery = risksQuery.Where(r => r.RiskType.Contains(riskType));
            }

            if (!string.IsNullOrEmpty(transactionReference))
            {
                risksQuery = risksQuery.Where(r => r.TransactionReference.Contains(transactionReference));
            }

            if (startDate.HasValue)
            {
                risksQuery = risksQuery.Where(r => r.AssessmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Correctly include the entire end day
                risksQuery = risksQuery.Where(r => r.AssessmentDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
            }

            risksQuery = risksQuery.OrderByDescending(r => r.AssessmentDate);

            return await risksQuery.ToListAsync();
        }

        /// <summary>
        /// Generates a risk management report for a specific user within a date range.
        /// </summary>
        /// <param name="startDate">Optional: The start date for the report.</param>
        /// <param name="endDate">Optional: The end date for the report.</param>
        /// <param name="userId">The ID of the user for whom to generate the report.</param>
        /// <returns>A RiskReportViewModel containing aggregated risk data.</returns>
        public async Task<RiskReportViewModel> GetReportAsync(DateTime? startDate, DateTime? endDate, int userId) // Added userId parameter
        {
            var actualStartDate = startDate ?? DateTime.Today.AddMonths(-3);
            var actualEndDate = endDate ?? DateTime.Today;

            IQueryable<Risk> query = _context.RiskAssessments
                                             .Where(r => r.UserId == userId); // Apply user filter for reports

            query = query.Where(r => r.AssessmentDate >= actualStartDate && r.AssessmentDate <= actualEndDate);

            // Fetch risks and calculate scores if they aren't stored in DB directly
            var filteredRisks = await query.OrderBy(r => r.AssessmentDate).ToListAsync();
            foreach (var risk in filteredRisks)
            {
                CalculateRiskScoreAndLevel(risk); // Ensure RiskScore and RiskLevel are populated for calculations
            }

            var totalRisks = filteredRisks.Count;
            // Ensure RiskScore is calculated for all risks before averaging
            var averageRiskScore = totalRisks > 0 ? filteredRisks.Average(r => r.RiskScore) : 0.0;

            // Updated risk level thresholds for clarity and distinct ranges
            var criticalRiskCount = filteredRisks.Count(r => r.RiskScore >= 20);
            var highRiskCount = filteredRisks.Count(r => r.RiskScore >= 15 && r.RiskScore < 20);
            var mediumRiskCount = filteredRisks.Count(r => r.RiskScore >= 10 && r.RiskScore < 15);
            var lowRiskCount = filteredRisks.Count(r => r.RiskScore >= 5 && r.RiskScore < 10);
            var veryLowRiskCount = filteredRisks.Count(r => r.RiskScore >= 1 && r.RiskScore < 5);


            return new RiskReportViewModel
            {
                StartDate = actualStartDate,
                EndDate = actualEndDate,
                TotalRisks = totalRisks,
                AverageRiskScore = averageRiskScore,
                // Include all new counts in the view model
                
                HighRiskCount = highRiskCount,
                MediumRiskCount = mediumRiskCount,
                LowRiskCount = lowRiskCount,
                CriticalRiskCount=criticalRiskCount,
                VeryLowRiskCount=veryLowRiskCount,
                FilteredRisks = filteredRisks
            };
        }
        /// <summary>
        /// Calculates RiskScore (Impact * Probability) and RiskLevel based on predefined thresholds.
        /// </summary>
        /// <param name="risk">The Risk object to calculate values for.</param>
        /// <returns>The Risk object with updated RiskScore and RiskLevel.</returns>
        public Risk CalculateRiskScoreAndLevel(Risk risk)
        {
            risk.RiskScore = (double)risk.Impact * risk.Probability;

            // Refined RiskLevel thresholds
            if (risk.RiskScore >= 20)
                risk.RiskLevel = "Critical";
            else if (risk.RiskScore >= 15)
                risk.RiskLevel = "High";
            else if (risk.RiskScore >= 10)
                risk.RiskLevel = "Medium";
            else if (risk.RiskScore >= 5)
                risk.RiskLevel = "Low";
            else
                risk.RiskLevel = "Very Low"; // Scores 1-4

            return risk;
        }

        /// <summary>
        /// Auto-suggests Impact and Probability based on Amount and RiskType.
        /// These values can be overridden by the treasurer.
        /// </summary>
        /// <param name="risk">The Risk object to suggest values for.</param>
        /// <returns>The Risk object with updated Impact, Probability, RiskScore, and RiskLevel.</returns>
        public Risk SuggestRiskValues(Risk risk)
        {
            // Auto-suggest Impact based on transaction amount
            if (risk.Amount > 100000m)
                risk.Impact = 5;      // Critical
            else if (risk.Amount > 50000m)
                risk.Impact = 4;      // High
            else if (risk.Amount > 20000m)
                risk.Impact = 3;      // Moderate
            else if (risk.Amount > 5000m) // Added a more granular lower threshold
                risk.Impact = 2;      // Low
            else
                risk.Impact = 1;      // Very Low for smaller amounts

            // Auto-suggest Probability based on risk type
            switch (risk.RiskType?.ToLower())
            {
                case "cybersecurity risk":
                case "market volatility":
                    risk.Probability = 4; // High likelihood
                    break;
                case "currency fluctuation":
                case "operational error":
                case "credit default":
                    risk.Probability = 3; // Moderate likelihood
                    break;
                case "liquidity risk":
                case "interest rate risk":
                    risk.Probability = 2; // Low likelihood
                    break;
                default:
                    risk.Probability = 1; // Very Low / Default for unknown types
                    break;
            }

            // Calculate Risk Score and Level after suggestion
            CalculateRiskScoreAndLevel(risk);

            return risk;
        }
        // Implementing the missing method 'GetAssessments' to satisfy the interface contract.
        public async Task<IEnumerable<object>> GetAssessments(DateTime startDate, DateTime endDate, int userId)
        {
            // Fetching risk assessments for the specified user within the date range.
            var assessments = await _context.RiskAssessments
                                            .Where(r => r.UserId == userId && r.AssessmentDate >= startDate && r.AssessmentDate <= endDate)
                                            .ToListAsync();

            // Returning the assessments as a collection of objects (or modify as needed based on actual requirements).
            return assessments.Select(r => new
            {
                r.RiskId,
                r.RiskType,
                r.AssessmentDate,
                r.RiskScore,
                r.RiskLevel
            });
        }
    }
}