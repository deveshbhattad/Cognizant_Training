// Models/Risk.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Required for [NotMapped]

namespace TMS_MAIN.Models
{
    public class Risk
    {
        [Key]

        [Required]
        [StringLength(20)]
        public string RiskId { get; set; } // PK, e.g., "RISK001"
        public int UserId { get; set; } // Link to User
        [ForeignKey("UserId")]
        public User User { get; set; }
        [Required]
        [StringLength(100)]
        public string RiskType { get; set; } // e.g., "Currency Fluctuation", "Credit Default"

        [Required]
        [StringLength(100)]
        public string TransactionReference { get; set; }

        [Required]
        [Range(0.0, 100.0, ErrorMessage = "Risk Score must be between 0 and 100.")]
        public double RiskScore { get; set; } // This IS stored in DB and calculated
        [Required]
        // Indicates this property is NOT mapped to a database column
      
        public decimal Amount { get; set; } // Used for calculation, not stored in Risk table
                                            // Indicates this property is NOT mapped to a database column
        [Required]
        [Range(1, 5, ErrorMessage = "Impact must be between 1 and 5.")]
        public int Impact { get; set; } // Used for calculation, not stored in Risk table

        // Indicates this property is NOT mapped to a database column
        [Required]
        [Range(1, 5, ErrorMessage = "Probability must be between 1 and 5.")]
        public int Probability { get; set; } // Used for calculation, not stored in Risk table

        // Indicates this property is NOT mapped to a database column
        // RiskLevel is derived from RiskScore, so it's not a required database field
        [Required]
        public string RiskLevel { get; set; }

        [Required]
        
        public DateTime AssessmentDate { get; set; } // Date of assessment
    }
}
