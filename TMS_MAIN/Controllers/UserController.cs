using Microsoft.AspNetCore.Mvc;

using TMS_MAIN.Data;

using TMS_MAIN.Models;
using TMS_MAIN.ViewModels; // Add this line
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Potentially for async operations if you use them

namespace TMS_MAIN.Controllers

{

    public class UserController : Controller
    {
        private readonly TreasuryManagementSystemContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(TreasuryManagementSystemContext context, ILogger<UserController> logger)
        {

            _context = context;
            _logger = logger;
        }

        [HttpGet]

        public IActionResult Register()

        {
            return View(); // This will now expect a UserRegisterViewModel if you follow convention
        }

        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (ModelState.IsValid)

            {
                // Check for existing username
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken");
                    return View(model);
                }

                // Check for existing email
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered");
                    return View(model);
                }

                try
                {
                    // In production, use proper password hashing!
                    var user = new User
                    {
                        Username = model.Username,
                        Password = model.Password, // Hash this in production!
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        IsAdmin = model.IsAdmin,
                        // Initialize navigation properties
                        CashFlows = new List<CashFlow>(),
                        BankAccounts = new List<BankAccount>(),
                        Investments = new List<Investment>(),
                        Reports = new List<Report>(),
                        Compliances = new List<Compliance>()
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "User registered successfully!";
                    return RedirectToAction("UserList");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering user");
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                }
            }

            // If we got here, something went wrong
            return View(model);
        }




        public IActionResult UserList()

        {

            var users = _context.Users.ToList();

            return View(users);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction("UserList");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("UserList");
            }

            try
            {
                
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User '{user.Username}' deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the user. Please try again.";
            }

            return RedirectToAction("UserList");
        }
    }

}