using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TMS_MAIN.Models
{
    public class BViewModel
    {
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Bank Name is required.")]
        [StringLength(100, ErrorMessage = "Bank Name can be at most 100 characters.")]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Account Number is required.")]
        [StringLength(50, ErrorMessage = "Account Number can be at most 50 characters.")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Account Type is required.")]
        [Display(Name = "Account Type")]
        public AccountType AccountType { get; set; }

        [Required(ErrorMessage = "Balance is required.")]
        [Range(0.00, 9999999999999.99, ErrorMessage = "Balance must be zero or greater.")]
        [Column(TypeName = "decimal(15,2)")]
        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Balance { get; set; }

        // Computed properties for display
        public string FormattedDateTime => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        public string DefaultUser => "tanujac2003";
    }
}