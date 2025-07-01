using System;
using System.Collections.Generic; // Added for ICollection
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS_MAIN.Models
{
    public class Report
    {
        [Key] // Designates ReportId as the primary key
        public int ReportId { get; set; }

        [Required(ErrorMessage = "Report Name is required.")] // Ensures ReportName is always set
        [StringLength(100, ErrorMessage = "Report Name cannot exceed 100 characters.")] // Maximum length for ReportName
        public string ReportName { get; set; } = string.Empty; // Initialize to empty string to avoid null warnings

        [Required(ErrorMessage = "Module is required.")] // Ensures Module is always set
        [StringLength(50, ErrorMessage = "Module cannot exceed 50 characters.")] // Maximum length for Module
        public string Module { get; set; } = string.Empty; // CashFlow, Investment, Risk. Initialize to empty string.

        [Required(ErrorMessage = "Start Date is required.")]
        [DataType(DataType.Date)] // Specifies that this property contains only date information
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End Date is required.")]
        [DataType(DataType.Date)] // Specifies that this property contains only date information
        public DateTime EndDate { get; set; }

        // GeneratedDate will automatically be set to the current time upon creation if not explicitly set.
        // It should not have [Required] as it's an auto-generated timestamp.
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [ForeignKey("UserId")] // Specifies UserId as a foreign key to the User table
        public virtual User User { get; set; } = null!; // Initialize as null-forgiving if it's always loaded with the Report

        // Parameters can be an optional JSON string. Using 'string?' makes it explicitly nullable.
        // If you always expect a JSON string, even an empty one, then keep it 'string' and initialize.
        public string? Parameters { get; set; } // JSON string to store additional parameters. Made nullable.

        [Required(ErrorMessage = "Status is required.")] // Ensures Status is always set
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")] // Maximum length for Status
        public string Status { get; set; } = "Generated"; // Default value. Initialize to default.

        // Navigation property for related Compliances.
        // Initialize the collection to prevent NullReferenceExceptions when adding items.
        public virtual ICollection<Compliance> Compliances { get; set; } = new List<Compliance>();
    }
}