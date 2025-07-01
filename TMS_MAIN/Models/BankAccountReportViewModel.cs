using System;
using System.Collections.Generic;
using System.Linq;
using TMS_MAIN.Models;

namespace TMS_MAIN.ViewModels
{
    public class BankAccountReportViewModel
    {
        public BankAccount BankAccount { get; set; }
        public IEnumerable<CashFlow> Transactions { get; set; } = new List<CashFlow>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Calculated properties
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }

        public decimal TotalDeposits => Transactions
            .Where(t => t.TransactionType == TransactionType.Inflow)
            .Sum(t => t.Amount);

        public decimal TotalWithdrawals => Transactions
            .Where(t => t.TransactionType == TransactionType.Outflow)
            .Sum(t => t.Amount);

        public decimal NetFlow => TotalDeposits - TotalWithdrawals;

        public int TotalTransactions => Transactions.Count();

        public bool HasData => BankAccount != null;
    }
}