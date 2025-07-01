using Microsoft.AspNetCore.Mvc;
using TMS_MAIN.Data;
using TMS_MAIN.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace TMS_MAIN.Controllers
{
    
    public class TreasurerController : Controller
    {
        private readonly TreasuryManagementSystemContext _context;

        public TreasurerController(TreasuryManagementSystemContext context)
        {
            _context = context;
        }

        public IActionResult TreasurerDashboard()
        {
            return View();
        }
        //just for redirection

        public IActionResult TreasurerProfile()
        {
            return View();
        }
        // GET: /Treasurer/TreasurerProfile
        // You can improve this by using authentication/session to get current user
        //[Authorize(AuthenticationSchemes = "MyCookieAuth")]
        //public IActionResult TreasurerProfile()
        //{
        //    var username = User.Identity?.Name;
        //    // For demo: get by username from query. In production, use User.Identity.Name, etc.
        //    if (string.IsNullOrEmpty(username))
        //    {

        //        return RedirectToAction("Login", "Account");


        //    }

        //    // Get the treasurer user (IsAdmin == false)
        //    var user = _context.Users.FirstOrDefault(u => u.Username == username && !u.IsAdmin);

        //    if (user == null)
        //    {
        //        // User not found or not a treasurer
        //        return NotFound();
        //    }

        //    // Pass user to view strongly-typed
        //    return View(user);
        //}
        public IActionResult About() => View();
        public IActionResult Contact() => View();
    }
}