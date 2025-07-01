using System.ComponentModel.DataAnnotations;

namespace TMS_MAIN.Models
{
    public class CFViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; }
        [Required]
        public int AccountId { get; set; }

        [Required]
        [Range(0.01, 9999999999999.99)]
        public decimal Amount { get; set; }

        [Required]
        public string TransactionType { get; set; }

        [Required]
        [StringLength(255)]
        public string Description { get; set; }

    }

}
