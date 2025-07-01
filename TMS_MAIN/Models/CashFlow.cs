using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS_MAIN.Models
{
    public class CashFlow
    {
        [Key]
        public int TransactionId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 9999999999999.99, ErrorMessage = "Amount must be greater than zero.")]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Transaction Type is required.")]
        public TransactionType TransactionType { get; set; }

        [Required(ErrorMessage = "Transaction Date is required.")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }

        [Required(ErrorMessage = "Description is required.")] // Make it required to match DB
        [StringLength(255, ErrorMessage = "Description can be at most 255 characters.")] 
        public string Description { get; set; }

        // Foreign key to User
        [Required]
        public int UserId { get; set; }

        // Navigation property & one cashflow belongs to one user
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [Required]
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual BankAccount BankAccount { get; set; }
    }
    public enum TransactionType 
    {
        Inflow,
        Outflow
    }
}