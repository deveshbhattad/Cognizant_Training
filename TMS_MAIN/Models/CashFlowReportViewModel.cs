
namespace TMS_MAIN.Models
{
    public class CashFlowReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public decimal TotalInflow { get; set; }
        public decimal TotalOutflow { get; set; }

        public string ErrorMessage { get; set; }
        public decimal NetCashFlow { get; set; }
        public int? AccountId { get; set; }
       // public string? AccountName { get; set; }
        public System.Collections.Generic.IEnumerable<CashFlow> CashFlows { get; set; }
        public IEnumerable<object> Transactions { get; internal set; }
    }
}
