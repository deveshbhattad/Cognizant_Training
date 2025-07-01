// Models/RiskScoreViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Added for IList, though not used in single lookup context

namespace TMS_MAIN.Models
{
    public class RiskScoreViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Enter Risk ID")]
        public string InputRiskId { get; set; } = string.Empty; // For user input to search

        // Properties to display the details of the found risk
        [Display(Name = "Risk ID")]
        public string? FoundRiskId { get; set; }

        [Display(Name = "Risk Type")]
        public string? FoundRiskType { get; set; }

        [Display(Name = "Transaction Reference")]
        public string? FoundTransactionReference { get; set; }

        [Display(Name = "Risk Score")]
        public double? FoundRiskScore { get; set; }

        [Display(Name = "Risk Level")] // Added for display based on RiskScore
        public string? FoundRiskLevel { get; set; } // Calculated level

        [DataType(DataType.Date)]
        [Display(Name = "Assessment Date")]
        public DateTime? FoundAssessmentDate { get; set; }

        public string? Message { get; set; } // For displaying messages (e.g., "Risk not found")
    }
}
