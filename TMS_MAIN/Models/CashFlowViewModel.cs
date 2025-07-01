namespace TMS_MAIN.Models
{
    public class CashFlowViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalInflow { get; set; }
        public decimal TotalOutflow { get; set; }
        public decimal NetCashFlow { get; set; }
        public System.Collections.Generic.IEnumerable<CashFlow> CashFlows { get; set; }
    }
}
