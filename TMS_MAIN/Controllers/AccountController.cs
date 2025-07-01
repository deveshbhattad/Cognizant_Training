using Microsoft.AspNetCore.Mvc;
using TMS_MAIN.Data;
using System.Linq;
using TMS_MAIN.Models;
using TMS_MAIN.ViewModels;

namespace TMS_MAIN.Controllers
{
    public class AccountController : Controller
    {
        private readonly TreasuryManagementSystemContext _context;
        private readonly ILogger<UserController> _logger;

        public AccountController(TreasuryManagementSystemContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string role, string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                ViewBag.Message = "Invalid credentials.";
                return View();
            }

            // Fix: Ensure HttpContext.Session is used correctly
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Username", user.Username);
            if (role == "admin" && user.IsAdmin)
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }
            else if (role == "treasurer" && !user.IsAdmin)
            {
               
                return RedirectToAction("TreasurerDashboard", "Treasurer");
            }
            else
            {
                ViewBag.Message = "Role and credentials do not match.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Force IsAdmin to false for new registrations
            var model = new UserRegisterViewModel { IsAdmin = false };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserRegisterViewModel model)
        {
            // Ensure new users can't register as admins
            model.IsAdmin = false;

            if (ModelState.IsValid)
            {
                // Check for existing username
                if (_context.Users.Any(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username is already taken");
                    return View(model);
                }

                // Check for existing email
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered");
                    return View(model);
                }

                try
                {
                    var user = new User
                    {
                        Username = model.Username,
                        Password = model.Password, // Remember to hash this in production!
                        FullName = model.FullName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        IsAdmin = false, // Force false for all new registrations
                        CashFlows = new List<CashFlow>(),
                        BankAccounts = new List<BankAccount>(),
                        Investments = new List<Investment>(),
                        Reports = new List<Report>(),
                        Compliances = new List<Compliance>()
                    };

                    _context.Users.Add(user);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Registration successful!";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                    // Log the error
                    _logger.LogError(ex, "Error registering user");
                }
            }

            return View(model);
        }

    }
}