using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TMS_MAIN.Data;
using TMS_MAIN.Models;
using TMS_MAIN.Services;
using TMS_MAIN.ViewModels;

namespace TMS_MAIN.Services
{
    public class ReportService : IReportService
    {
        private readonly TreasuryManagementSystemContext _context;
        private readonly ITransactionService _transactionService;
        private readonly IInvestmentService _investmentService;
        private readonly IRiskAssessmentService _riskAssessmentService;

        public ReportService(
            TreasuryManagementSystemContext context,
            ITransactionService transactionService,
            IInvestmentService investmentService,
            IRiskAssessmentService riskAssessmentService)
        {
            _context = context;
            _transactionService = transactionService;
            _investmentService = investmentService;
            _riskAssessmentService = riskAssessmentService;
        }

        public Report GenerateReport(Report report)
        {
            _context.Reports.Add(report);
            _context.SaveChanges();
            return report;
        }

        public IEnumerable<Report> GetReportsByUserId(int userId)
        {
            return _context.Reports
                .Include(r => r.User)
                .Include(r => r.Compliances)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.GeneratedDate)
                .ToList();
        }

        public Report GetReportById(int id, int userId)
        {
            return _context.Reports
                .Include(r => r.User)
                .Include(r => r.Compliances)
                .ThenInclude(c => c.User)
                .FirstOrDefault(r => r.ReportId == id && r.UserId == userId);
        }

        public void UpdateReportStatus(int reportId, string status)
        {
            var report = _context.Reports.Find(reportId);
            if (report != null)
            {
                report.Status = status;
                _context.SaveChanges();
            }
        }

        public Compliance SubmitCompliance(Compliance compliance)
        {
            _context.Compliances.Add(compliance);

            // Update report status
            var report = _context.Reports.Find(compliance.ReportId);
            if (report != null)
            {
                report.Status = "ComplianceSubmitted";
                _context.SaveChanges();
            }

            return compliance;
        }

        public IEnumerable<Compliance> GetCompliancesByReportId(int reportId)
        {
            return _context.Compliances
                .Include(c => c.User)
                .Where(c => c.ReportId == reportId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();
        }

        public object GenerateReportData(Report report)
        {
            try
            {
                dynamic parameters = JsonConvert.DeserializeObject(report.Parameters ?? "{}");
                int accountId = parameters?.AccountId ?? 0;

                switch (report.Module)
                {
                    case "CashFlow":
                        return _transactionService.GetReport(
                            report.StartDate,
                            report.EndDate,
                            accountId,
                            report.UserId)
                            ?? new CashFlowReportViewModel();

                    case "Investment":
                        return _investmentService.GetPortfolioSummary(report.UserId)
                            ?? new PortfolioSummaryViewModel();

                    case "Risk":
                        return _riskAssessmentService.GetReportAsync(
                            report.StartDate,
                            report.EndDate,
                            report.UserId).Result
                            ?? new RiskReportViewModel();

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating report data: {ex.Message}");
                return report.Module switch
                {
                    "CashFlow" => new CashFlowReportViewModel(),
                    "Investment" => new PortfolioSummaryViewModel(),
                    "Risk" => new RiskReportViewModel(),
                    _ => null
                };
            }
        }
        public IEnumerable<Compliance> GetCompliancesByUserId(int userId)
        {
            return _context.Compliances
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToList();
        }
        public bool DeleteReport(int reportId, int userId)
        {
            try
            {
                var report = _context.Reports
                    .FirstOrDefault(r => r.ReportId == reportId && r.UserId == userId);

                if (report == null)
                    return false;

                _context.Reports.Remove(report);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                // Log error here if needed
                return false;
            }
        }


    }
}
