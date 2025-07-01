using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS_MAIN.Models
{
    public class Compliance
    {
        [Key]
        public int ComplianceId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [ForeignKey("ReportId")]
        public virtual Report Report { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted"; // Submitted, Reviewed, Approved

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }
    }
}