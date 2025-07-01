
using System;
using System.ComponentModel.DataAnnotations;

namespace TMS_MAIN.ViewModels
{
    public class IViewModel
    {
        [Required]
        [Display(Name = "InvestmentType")]
        public string InvestmentType { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Amount Invested must be a positive number.")]
        [Display(Name = "Amount Invested")]
        public decimal AmountInvested { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Current Value must be a positive number.")]
        [Display(Name = "Current Value")]
        public decimal CurrentValue { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Maturity Date")]
        public DateTime MaturityDate { get; set; }
    }
}

