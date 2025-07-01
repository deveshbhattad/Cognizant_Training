
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS_MAIN.Models
{
    public class Investment
    {
        [Key] // Designates InvestmentId as the primary key
        public int InvestmentId { get; set; }

        [Required] // Ensures InvestmentType is always set
        public InvestmentType InvestmentType { get; set; }

        [Required] // Ensures AmountInvested is always set
        [Column(TypeName = "decimal(18, 2)")] // Explicitly maps to decimal with 18 precision and 2 scale
        public decimal AmountInvested { get; set; }

        [Required] // Ensures CurrentValue is always set
        [Column(TypeName = "decimal(18, 2)")] // Explicitly maps to decimal with 18 precision and 2 scale
        public decimal CurrentValue { get; set; }

        [Required] // Ensures PurchaseDate is always set
        public DateTime PurchaseDate { get; set; }

        public DateTime? MaturityDate { get; set; } // Made nullable, as not all investments have a maturity date

        [Required] // Ensures UserId is always set for the foreign key relationship
        public int UserId { get; set; }

        // Navigation property to the User who owns this investment
        public User User { get; set; }
    }

    public enum InvestmentType
    {
        Bonds,       // 0
        Equities,    // 1
        MutualFunds, // 2
         
    }

}
