// Models/RiskReportViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TMS_MAIN.Models
{
    public class RiskReportViewModel
    {
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddMonths(-3); // Default to last 3 months

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today; // Default to today

        public int TotalRisks { get; set; }
        public double AverageRiskScore { get; set; } // Re-added
        public int CriticalRiskCount { get; set; }
        public int HighRiskCount { get; set; } // Re-added
        public int MediumRiskCount { get; set; } // Re-added
        public int LowRiskCount { get; set; } // Re-added
        public int VeryLowRiskCount { get; set; }
        public IList<Risk> FilteredRisks { get; set; } = new List<Risk>(); // Changed to RiskAssessment
        public object ErrorMessage { get; internal set; }
    }
}