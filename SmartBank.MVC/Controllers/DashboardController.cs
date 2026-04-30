using Microsoft.AspNetCore.Mvc;

namespace SmartBank.MVC.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        // Check if JWT cookie exists — if not, redirect to login
        if (!Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Login", "Auth");

        if (Request.Cookies.TryGetValue("SmartBankRole", out var role) &&
            (role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || role.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
            return RedirectToAction("Index", "Admin");

        return View();
    }
}
