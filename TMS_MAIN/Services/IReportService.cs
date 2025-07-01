using System.Collections.Generic;
using TMS_MAIN.Models;

namespace TMS_MAIN.Services
{
    public interface IReportService
    {
        Report GenerateReport(Report report);
        IEnumerable<Report> GetReportsByUserId(int userId);
        Report GetReportById(int id, int userId);
        void UpdateReportStatus(int reportId, string status);
        Compliance SubmitCompliance(Compliance compliance);
        IEnumerable<Compliance> GetCompliancesByUserId(int userId);
        IEnumerable<Compliance> GetCompliancesByReportId(int reportId);
        object GenerateReportData(Report report);

        public bool DeleteReport(int reportId, int userId);


    }
}