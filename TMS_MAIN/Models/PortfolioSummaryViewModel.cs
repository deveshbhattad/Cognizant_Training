
using System;
using System.Collections.Generic;

namespace TMS_MAIN.ViewModels
{
    public class PortfolioSummaryViewModel
    {
        public decimal TotalInvested { get; set; }
        public decimal TotalCurrentValue { get; set; }
        public decimal AmountInvested { get; set; }
        public decimal CurrentValue { get; set; }

        public string ErrorMessage { get; set; }
        public decimal TotalReturns => TotalCurrentValue - TotalInvested;

        public double ROI => TotalInvested == 0 ? 0 : (double)(TotalReturns / TotalInvested) * 100;

        //public double ROI => AmountInvested == 0 ? 0 : (double)((CurrentValue - AmountInvested) / AmountInvested) * 100;

        public int NumberOfInvestments { get; set; }

        public decimal HighestInvestmentValue { get; set; }
        public decimal LowestInvestmentValue { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<InvestmentBreakdownViewModel> InvestmentBreakdown { get; set; }
    }

    public class InvestmentBreakdownViewModel
    {
        public string InvestmentType { get; set; }
        public decimal AmountInvested { get; set; }
        public decimal CurrentValue { get; set; }
    }
}
