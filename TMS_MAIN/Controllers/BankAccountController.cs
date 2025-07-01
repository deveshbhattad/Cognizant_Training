using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TMS_MAIN.Services;
using TMS_MAIN.Models;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace TMS_MAIN.Controllers
{
    public class BankAccountController : Controller
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(
            IBankAccountService bankAccountService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BankAccountController> logger)
        {
            _bankAccountService = bankAccountService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                _logger.LogInformation($"Attempting to load bank accounts. User ID from session: {loggedInUserId}");

                if (!loggedInUserId.HasValue)
                {
                    _logger.LogWarning("No user ID found in session. Redirecting to login.");
                    return RedirectToAction("Login", "Account");
                }

                string currentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";
                string currentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                var bankAccounts = _bankAccountService.GetBankAccountsByUserId(loggedInUserId.Value);
                var totalBalance = bankAccounts.Sum(a => a.Balance);

                var model = new BankAccountViewModel
                {
                    CurrentDateTime = currentDateTime,
                    CurrentUser = currentUser,
                    BankAccounts = bankAccounts ?? new List<BankAccount>(),
                    TotalBalance = totalBalance
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Index action: {ex.Message}");
                TempData["Error"] = "Failed to load bank accounts.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Add()
        {
            try
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";

                // Create SelectList for AccountTypes enum
                ViewBag.AccountTypes = new SelectList(Enum.GetValues(typeof(AccountType)));

                return View(new BViewModel());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Add GET action: {ex.Message}");
                TempData["Error"] = "Failed to load add bank account form.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(BViewModel viewModel)
        {
            try
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (ModelState.IsValid)
                {
                    var bankAccount = new BankAccount
                    {
                        BankName = viewModel.BankName,
                        AccountNumber = viewModel.AccountNumber,
                        AccountType = viewModel.AccountType,
                        Balance = viewModel.Balance,
                        UserId = loggedInUserId.Value
                    };

                    _bankAccountService.AddBankAccount(bankAccount);
                    TempData["Success"] = "Bank account added successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // If we got here, something failed; redisplay form
                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";
                ViewBag.AccountTypes = new SelectList(Enum.GetValues(typeof(AccountType)));

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding bank account: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while adding the bank account.");

                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";
                ViewBag.AccountTypes = new SelectList(Enum.GetValues(typeof(AccountType)));

                return View(viewModel);
            }
        }



        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                var bankAccount = _bankAccountService.GetBankAccountById(id);
                if (bankAccount == null || bankAccount.UserId != loggedInUserId)
                {
                    return NotFound();
                }

                var viewModel = new BViewModel
                {
                    AccountId = bankAccount.AccountId,
                    BankName = bankAccount.BankName,
                    AccountNumber = bankAccount.AccountNumber,
                    AccountType = bankAccount.AccountType,
                    Balance = bankAccount.Balance
                };

                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";
                ViewBag.AccountTypes = new SelectList(Enum.GetValues(typeof(AccountType)));

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Edit GET action: {ex.Message}");
                TempData["Error"] = "Failed to load bank account details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, BViewModel viewModel)
        {
            try
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (id != viewModel.AccountId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var bankAccount = _bankAccountService.GetBankAccountById(id);
                    if (bankAccount == null || bankAccount.UserId != loggedInUserId)
                    {
                        return NotFound();
                    }

                    bankAccount.BankName = viewModel.BankName;
                    bankAccount.AccountNumber = viewModel.AccountNumber;
                    bankAccount.AccountType = viewModel.AccountType;
                    bankAccount.Balance = viewModel.Balance;
                    bankAccount.UserId = loggedInUserId.Value;

                    _bankAccountService.Update(bankAccount);
                    TempData["Success"] = "Bank account updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";
                ViewBag.AccountTypes = new SelectList(Enum.GetValues(typeof(AccountType)));

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating bank account: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the bank account.");

                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";
                ViewBag.AccountTypes = new SelectList(Enum.GetValues(typeof(AccountType)));

                return View(viewModel);
            }
        }

       
        
        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    _logger.LogWarning("Delete attempted with null ID");
                    return NotFound();
                }

                // Check if user is logged in
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    _logger.LogWarning("Unauthorized delete attempt - user not logged in");
                    return RedirectToAction("Login", "Account");
                }

                var bankAccount = _bankAccountService.GetBankAccountById(id.Value);
                if (bankAccount == null)
                {
                    _logger.LogWarning($"Bank account with ID {id} not found for deletion");
                    return NotFound();
                }

                // Check if the logged-in user owns this account
                if (bankAccount.UserId != loggedInUserId.Value)
                {
                    _logger.LogWarning($"Unauthorized delete attempt for bank account {id} by user {loggedInUserId}");
                    return Forbid();
                }

                // Add the current user and datetime to ViewBag
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";
                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                return View(bankAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete GET action: {ex.Message}");
                TempData["Error"] = "An error occurred while processing your request.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: BankAccount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                // Check if user is logged in
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    _logger.LogWarning("Unauthorized delete confirmation attempt - user not logged in");
                    return RedirectToAction("Login", "Account");
                }

                var bankAccount = _bankAccountService.GetBankAccountById(id);
                if (bankAccount == null)
                {
                    _logger.LogWarning($"Bank account with ID {id} not found for deletion confirmation");
                    return NotFound();
                }

                // Check if the logged-in user owns this account
                if (bankAccount.UserId != loggedInUserId.Value)
                {
                    _logger.LogWarning($"Unauthorized delete confirmation attempt for bank account {id} by user {loggedInUserId}");
                    return Forbid();
                }

                _bankAccountService.Delete(id);
                _logger.LogInformation($"Bank account {id} successfully deleted by user {loggedInUserId}");
                TempData["Success"] = "Bank account successfully deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Delete POST action: {ex.Message}");
                TempData["Error"] = "An error occurred while deleting the bank account.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Reconcile(int id)
        {
            try
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                var bankAccount = _bankAccountService.GetBankAccountById(id);
                if (bankAccount == null || bankAccount.UserId != loggedInUserId)
                {
                    TempData["Error"] = "Bank account not found or access denied.";
                    return RedirectToAction("Index");
                }

                ViewBag.CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
                ViewBag.CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003";

                return View(new ReconciliationModel
                {
                    BankAccountId = bankAccount.AccountId,
                    BankName = bankAccount.BankName,
                    AccountNumber = bankAccount.AccountNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Reconcile GET action: {ex.Message}");
                TempData["Error"] = "Failed to load reconciliation page.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reconcile(int id, IFormFile bankStatement, IFormFile internalTransactions)
        {
            try
            {
                int? loggedInUserId = _httpContextAccessor.HttpContext?.Session?.GetInt32("UserId");
                if (!loggedInUserId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                var bankAccount = _bankAccountService.GetBankAccountById(id);
                if (bankAccount == null || bankAccount.UserId != loggedInUserId)
                {
                    TempData["Error"] = "Bank account not found or access denied.";
                    return RedirectToAction("Index");
                }

                if (bankStatement == null || internalTransactions == null)
                {
                    TempData["Error"] = "Please upload both CSV files.";
                    return RedirectToAction("Reconcile", new { id });
                }

                var bankTransactions = await ReadCsvTransactions(bankStatement);
                var internalTxs = await ReadCsvTransactions(internalTransactions);

                var result = new ReconciliationResult
                {
                    BankName = bankAccount.BankName,
                    AccountNumber = bankAccount.AccountNumber,
                    CurrentDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
                    CurrentUser = _httpContextAccessor.HttpContext?.Session?.GetString("Username") ?? "tanujac2003"
                };

                // Perform reconciliation
                foreach (var bankTx in bankTransactions)
                {
                    var match = internalTxs.FirstOrDefault(i =>
                        i.TransactionDate.Date == bankTx.TransactionDate.Date &&
                        i.Amount == bankTx.Amount &&
                        i.TransactionType == bankTx.TransactionType &&
                        !i.IsMatched);

                    if (match != null)
                    {
                        bankTx.IsMatched = true;
                        match.IsMatched = true;
                        result.MatchedTransactions.Add(bankTx);
                    }
                    else
                    {
                        result.UnmatchedBankTransactions.Add(bankTx);
                    }
                }

                result.UnmatchedInternalTransactions = internalTxs.Where(t => !t.IsMatched).ToList();
                result.TotalDiscrepancy = bankTransactions.Sum(t => t.Amount) - internalTxs.Sum(t => t.Amount);

                return View("ReconciliationResult", result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Reconcile POST action: {ex.Message}");
                TempData["Error"] = "Failed to process reconciliation.";
                return RedirectToAction("Index");
            }
        }

        private async Task<List<TransactionRecord>> ReadCsvTransactions(IFormFile file)
        {
            var transactions = new List<TransactionRecord>();
            using var reader = new StreamReader(file.OpenReadStream());

            // Skip header
            await reader.ReadLineAsync();

            while (await reader.ReadLineAsync() is string line)
            {
                var values = line.Split(',');
                if (values.Length >= 4 &&
                    DateTime.TryParse(values[0], out DateTime date) &&
                    decimal.TryParse(values[2], out decimal amount))
                {
                    transactions.Add(new TransactionRecord
                    {
                        TransactionDate = date,
                        TransactionType = values[1],
                        Amount = amount,
                        Description = values[3],
                        IsMatched = false
                    });
                }
            }
            return transactions;
        }


        
    }
}