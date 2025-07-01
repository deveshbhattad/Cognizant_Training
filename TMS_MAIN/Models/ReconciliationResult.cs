namespace TMS_MAIN.Models
{
    public class ReconciliationResult
    {
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string CurrentDateTime { get; set; }
        public string CurrentUser { get; set; }
        public List<TransactionRecord> MatchedTransactions { get; set; } = new();
        public List<TransactionRecord> UnmatchedBankTransactions { get; set; } = new();
        public List<TransactionRecord> UnmatchedInternalTransactions { get; set; } = new();
        public decimal TotalDiscrepancy { get; set; }
    }
}
