using Microsoft.EntityFrameworkCore;
using System.Linq;
using TMS_MAIN.Data;
using TMS_MAIN.Models;
using System.Collections.Generic;

namespace TMS_MAIN.Services
{
    public interface ITransactionService
    {
        List<CashFlow> GetCashFlows();//For Admin                                    
        bool RecordTransaction(CashFlow cashFlow, out string errorMessage);
        CashFlow GetCashFlowById(int id);       
        void Update(CashFlow cashFlow);
        void Delete(int id);
        decimal GetTotalInflow(DateTime startDate, DateTime endDate);
        decimal GetTotalOutflow(DateTime startDate, DateTime endDate);
        decimal GetNetCashFlow(DateTime startDate, DateTime endDate);
        IEnumerable<CashFlow> GetCashFlows(DateTime startDate, DateTime endDate);// It is used
        IEnumerable<CashFlow> GetCashFlowsByUserId(int userId); // only cashflows of one user
        CashFlowReportViewModel GetReport(DateTime startDate, DateTime endDate, int accountId, int userId);

        //bank methods
        BankAccount GetBankAccountById(int accountid);
    }

    public class CashFlowService : ITransactionService
    {
        private readonly TreasuryManagementSystemContext _context;

        public CashFlowService(TreasuryManagementSystemContext context)
        {
            _context = context;
        }       
        public bool RecordTransaction(CashFlow cashFlow, out string errorMessage)
        {
            errorMessage = null;
            var bankAccount = _context.BankAccounts.FirstOrDefault(b => b.AccountId == cashFlow.AccountId && b.UserId == cashFlow.UserId);
            if (bankAccount == null)
            {
                errorMessage = "Bank Account Not Found.";
                return false;
            }

            if (cashFlow.TransactionType == TransactionType.Outflow && bankAccount.Balance < cashFlow.Amount)
            {
                errorMessage = "Insufficient Balance For This outflow Transaction.";
                return false;
            }

            if (cashFlow.TransactionType == TransactionType.Inflow)
                bankAccount.Balance += cashFlow.Amount;
            else
                bankAccount.Balance -= cashFlow.Amount;

            _context.CashFlows.Add(cashFlow);
            _context.BankAccounts.Update(bankAccount);
            _context.SaveChanges();
            return true;//returns true after success
        }


        public List<CashFlow> GetCashFlows()
        {
            return _context.CashFlows.Include(c => c.User).OrderByDescending(d => d.TransactionDate).ToList();
        }

        public CashFlow GetCashFlowById(int id)
        {
            return _context.CashFlows.FirstOrDefault(t => t.TransactionId == id);
        }       

        public void Update(CashFlow cashFlow)
        {
            var existingCashFlow = _context.CashFlows.Find(cashFlow.TransactionId);

            if (existingCashFlow != null)
            {
                _context.Entry(existingCashFlow).CurrentValues.SetValues(cashFlow);
                _context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            var cf = _context.CashFlows.Include(c => c.User).FirstOrDefault(t => t.TransactionId == id);
            if (cf != null)
            {
                _context.CashFlows.Remove(cf);
                _context.SaveChanges();
            }
        }

        public decimal GetTotalInflow(DateTime startDate, DateTime endDate)
        {
            return _context.CashFlows
                           .Where(c => c.TransactionType == TransactionType.Inflow &&
                                       c.TransactionDate >= startDate &&
                                       c.TransactionDate <= endDate)
                           .Sum(c => c.Amount);
        }

        public decimal GetTotalOutflow(DateTime startDate, DateTime endDate)
        {
            return _context.CashFlows
                           .Where(c => c.TransactionType == TransactionType.Outflow &&
                                       c.TransactionDate >= startDate &&
                                       c.TransactionDate <= endDate)
                           .Sum(c => c.Amount);
        }

        public decimal GetNetCashFlow(DateTime startDate, DateTime endDate)
        {
            var inflow = GetTotalInflow(startDate, endDate);
            var outflow = GetTotalOutflow(startDate, endDate);
            return inflow - outflow;
        }

        public IEnumerable<CashFlow> GetCashFlows(DateTime startDate, DateTime endDate)
        {
            return _context.CashFlows
                           //.Include(c => c.User)include is used when we want to load user data also
                           .Where(c => c.TransactionDate >= startDate && c.TransactionDate <= endDate)
                           .OrderBy(c => c.TransactionDate)
                           .ToList();
        }

        public IEnumerable<CashFlow> GetCashFlowsByUserId(int userId)
        {
            return _context.CashFlows
                          // .Include(c => c.User)
                           .Where(cf => cf.UserId == userId)
                           .OrderByDescending(d => d.TransactionDate)
                           .ToList();
        }

        //Bank methods
        public BankAccount GetBankAccountById(int accountid)
        {
            return _context.BankAccounts.FirstOrDefault(b => b.AccountId == accountid);
        }

        public CashFlowReportViewModel GetReport(DateTime startDate, DateTime endDate, int accountId, int userId)
        {
            var transactions = _context.CashFlows
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .Where(t => accountId == 0 || t.AccountId == accountId)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
            var totalInflow = transactions.Where(t => t.TransactionType == TransactionType.Inflow).Sum(t => t.Amount);
            var totalOutflow = transactions.Where(t => t.TransactionType == TransactionType.Outflow).Sum(t => t.Amount);
            var netCashFlow = totalInflow - totalOutflow;
            return new CashFlowReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                AccountId = accountId,
                CashFlows = transactions,
                TotalInflow = totalInflow,
                TotalOutflow = totalOutflow,
                NetCashFlow = netCashFlow,
                GeneratedDate = DateTime.Now
            };
        }



    }
}