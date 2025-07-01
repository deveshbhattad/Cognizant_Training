using TMS_MAIN.Models;

namespace TMS_MAIN.Services
{
    public interface IRiskAssessmentService
    {
        Task<IList<Risk>> GetAllAsync(int userId);
        Task<Risk?> GetByIdAsync(string id,int userId);
        Task AddAsync(Risk risk);
        Task UpdateAsync(Risk risk,int userId);
        Task DeleteAsync(string id,int userId);
        Task<IList<Risk>> GetFilteredAsync(string? riskId, string? riskType, string? transactionReference, DateTime? startDate, DateTime? endDate,int userId);
        Task<RiskReportViewModel> GetReportAsync(DateTime? startDate, DateTime? endDate,int userId);

        // New methods for auto-suggestion and risk calculation
        Risk CalculateRiskScoreAndLevel(Risk risk);
        Risk SuggestRiskValues(Risk risk);
        Task<IEnumerable<object>> GetAssessments(DateTime startDate, DateTime endDate, int userId);
    }
}
