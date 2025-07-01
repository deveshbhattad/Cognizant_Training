namespace TMS_MAIN.Models
{
    public class TransactionRecord
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public bool IsMatched { get; set; }
    }
}
