
using System.Collections.Generic;
using TMS_MAIN.Models;

namespace TMS_MAIN.ViewModels
{
    public class InvestmentSummaryViewModel
    {
        public decimal TotalInvested { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal GainLoss { get; set; }
        public List<Investment> Investments { get; set; }
    }
}
