using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS_MAIN.Models
{
    public enum AccountType // Assuming this enum exists, or you need to define it
    {
        Savings,
        Checking,
        Current,
        Other
    }

    public class BankAccount
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [StringLength(100)]
        public string BankName { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        // This is the property causing errors if missing or named differently
        public AccountType AccountType { get; set; }

        // This is the property causing errors if missing or named differently
        [Column(TypeName = "decimal(18, 2)")] // Already configured in DbContext, but good to have here too
        public decimal Balance { get; set; }

        // This is the foreign key property causing errors if missing or named differently
        public int UserId { get; set; }

        // Navigation property for the User
        public User User { get; set; }

        // Navigation property for CashFlows (assuming one-to-many)
        public ICollection<CashFlow> CashFlows { get; set; } // Changed to ICollection
    }
}