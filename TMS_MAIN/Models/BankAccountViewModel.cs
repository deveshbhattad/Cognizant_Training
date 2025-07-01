namespace TMS_MAIN.Models
{
    public class BankAccountViewModel
    {
        public string CurrentDateTime { get; set; }
        public string CurrentUser { get; set; }
        public decimal ExpectedBalance { get; set; }
        public decimal ActualBalance { get; set; }
        public decimal Discrepancy { get; set; }
        public decimal TotalBalance { get; set; }
        public System.Collections.Generic.IEnumerable<BankAccount> BankAccounts { get; set; }

        public BankAccountViewModel()
        {
            BankAccounts = new List<BankAccount>();
        }
    }
}