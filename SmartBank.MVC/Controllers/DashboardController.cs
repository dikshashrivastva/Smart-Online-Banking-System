using Microsoft.AspNetCore.Mvc;

namespace SmartBank.MVC.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index()
    {
        // Check if JWT cookie exists — if not, redirect to login
        if (!Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Login", "Auth");

        return View();
    }
}
