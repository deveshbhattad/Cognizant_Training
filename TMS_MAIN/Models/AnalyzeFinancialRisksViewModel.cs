// Models/AnalyzeFinancialRisksViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMS_MAIN.Models
{
    public class AnalyzeFinancialRisksViewModel
    {
        public string? RiskId { get; set; }
        public string? RiskType { get; set; }
        // Removed: public string? TransactionReference { get; set; } // Not used in this approach
        public string TransactionReference { get; set; } = string.Empty;
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public IList<Risk> Risks { get; set; } = new List<Risk>();
    }
}
