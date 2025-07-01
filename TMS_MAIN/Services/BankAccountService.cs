using Microsoft.EntityFrameworkCore;
using TMS_MAIN.Data;
using TMS_MAIN.Models;
using TMS_MAIN.ViewModels;

namespace TMS_MAIN.Services
{
    public interface IBankAccountService
    {
        List<BankAccount> GetBankAccounts();
        void AddBankAccount(BankAccount bankAccount);
        BankAccount GetBankAccountById(int id);
        void Update(BankAccount bankAccount);
        void Delete(int id);
        IEnumerable<BankAccount> GetBankAccountsByUserId(int userId);

        BankAccountReportViewModel GetBankAccountReport(DateTime startDate, DateTime endDate, int accountId, int userId);


        bool AccountExists(int accountId, int userId);


    }

    public class BankAccountService : IBankAccountService
    {
        private readonly TreasuryManagementSystemContext _context;
        private readonly ILogger<BankAccountService> _logger;

        public BankAccountService(TreasuryManagementSystemContext context, ILogger<BankAccountService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void AddBankAccount(BankAccount bankAccount)
        {
            try
            {
                if (bankAccount == null)
                {
                    _logger.LogError("Attempted to add null bank account");
                    throw new ArgumentNullException(nameof(bankAccount));
                }

                _logger.LogInformation($"Adding new bank account for user {bankAccount.UserId}");
                _context.BankAccounts.Add(bankAccount);

                var saveResult = _context.SaveChanges();
                if (saveResult > 0)
                {
                    _logger.LogInformation($"Successfully added bank account with ID: {bankAccount.AccountId}");
                }
                else
                {
                    _logger.LogWarning("SaveChanges returned 0 - no bank account was added to the database");
                    throw new Exception("Failed to save bank account to database");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database error while adding bank account: {dbEx.Message}");
                throw new Exception("A database error occurred while saving the bank account.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding bank account: {ex.Message}");
                throw;
            }
        }

        public List<BankAccount> GetBankAccounts()
        {
            try
            {
                _logger.LogInformation("Retrieving all bank accounts");
                return _context.BankAccounts
                    .Include(b => b.User)
                    .OrderByDescending(b => b.AccountId)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving all bank accounts: {ex.Message}");
                return new List<BankAccount>();
            }
        }

        public BankAccount GetBankAccountById(int id)
        {
            try
            {
                _logger.LogInformation($"Retrieving bank account with ID: {id}");
                return _context.BankAccounts
                    .Include(b => b.User)
                    .FirstOrDefault(b => b.AccountId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving bank account with ID {id}: {ex.Message}");
                return null;
            }
        }

        public void Update(BankAccount bankAccount)
        {
            try
            {
                if (bankAccount == null)
                {
                    throw new ArgumentNullException(nameof(bankAccount));
                }

                _logger.LogInformation($"Updating bank account with ID: {bankAccount.AccountId}");
                var existingAccount = _context.BankAccounts.Find(bankAccount.AccountId);

                if (existingAccount == null)
                {
                    throw new KeyNotFoundException($"Bank account with ID {bankAccount.AccountId} not found");
                }

                _context.Entry(existingAccount).CurrentValues.SetValues(bankAccount);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    _logger.LogInformation($"Successfully updated bank account with ID: {bankAccount.AccountId}");
                }
                else
                {
                    _logger.LogWarning($"No changes were saved for bank account with ID: {bankAccount.AccountId}");
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database error while updating bank account: {dbEx.Message}");
                throw new Exception("A database error occurred while updating the bank account.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating bank account {bankAccount?.AccountId}: {ex.Message}");
                throw;
            }
        }

        public void Delete(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete bank account with ID: {id}");
                var bankAccount = _context.BankAccounts.Find(id);

                if (bankAccount != null)
                {
                    _context.BankAccounts.Remove(bankAccount);
                    var result = _context.SaveChanges();

                    if (result > 0)
                    {
                        _logger.LogInformation($"Successfully deleted bank account with ID: {id}");
                    }
                    else
                    {
                        _logger.LogWarning($"No bank account was deleted with ID: {id}");
                    }
                }
                else
                {
                    _logger.LogWarning($"Bank account with ID {id} not found for deletion");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting bank account {id}: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<BankAccount> GetBankAccountsByUserId(int userId)
        {
            try
            {
                return _context.BankAccounts
                    .Where(b => b.UserId == userId)
                    .OrderBy(b => b.AccountId)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving bank accounts for user {userId}: {ex.Message}");
                return new List<BankAccount>();
            }
        }
        public BankAccountReportViewModel GetBankAccountReport(
            DateTime startDate,
            DateTime endDate,
            int accountId,
            int userId)
        {
            _logger.LogInformation($"Getting bank account report for AccountId: {accountId}, UserId: {userId}");

            try
            {
                // Explicitly load bank account with transactions
                var bankAccount = _context.BankAccounts
                    .AsNoTracking()
                    .Include(ba => ba.CashFlows)
                    .FirstOrDefault(ba => ba.AccountId == accountId && ba.UserId == userId);

                if (bankAccount == null)
                {
                    _logger.LogWarning($"Bank account not found - AccountId: {accountId}, UserId: {userId}");
                    return new BankAccountReportViewModel
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    };
                }

                _logger.LogInformation($"Found bank account: {bankAccount.BankName}");

                // Get transactions in date range
                var transactions = bankAccount.CashFlows
                    .Where(cf => cf.TransactionDate >= startDate && cf.TransactionDate <= endDate)
                    .OrderBy(cf => cf.TransactionDate)
                    .ToList();

                _logger.LogInformation($"Found {transactions.Count} transactions in date range");

                // Calculate opening balance (balance before the report period)
                var openingBalance = CalculateOpeningBalance(bankAccount, startDate);

                return new BankAccountReportViewModel
                {
                    BankAccount = bankAccount,
                    Transactions = transactions,
                    StartDate = startDate,
                    EndDate = endDate,
                    OpeningBalance = openingBalance,
                    ClosingBalance = bankAccount.Balance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating bank account report for AccountId: {accountId}");
                return new BankAccountReportViewModel
                {
                    StartDate = startDate,
                    EndDate = endDate
                };
            }
        }

        private decimal CalculateOpeningBalance(BankAccount account, DateTime startDate)
        {
            // Get all transactions before the report period
            var transactionsBefore = account.CashFlows
                .Where(cf => cf.TransactionDate < startDate)
                .ToList();

            // Start with current balance and work backwards
            var balance = account.Balance;

            foreach (var transaction in transactionsBefore.OrderByDescending(t => t.TransactionDate))
            {
                balance -= transaction.TransactionType == TransactionType.Inflow
                    ? transaction.Amount
                    : -transaction.Amount;
            }

            return balance;
        }

        public bool AccountExists(int accountId, int userId)
        {
            return _context.BankAccounts
                .Any(a => a.AccountId == accountId && a.UserId == userId);
        }
    }
}