using Microsoft.AspNetCore.Mvc;
using SmartBank.Models.DTOs.Auth;
using System.Net.Http.Json;
using System.Text.Json;

namespace SmartBank.MVC.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration     _config;

    public AuthController(IHttpClientFactory httpFactory, IConfiguration config)
    {
        _httpFactory = httpFactory;
        _config      = config;
    }

    // ─── GET /Auth/Login ──────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    // ─── POST /Auth/Login ─────────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _httpFactory.CreateClient("SmartBankAPI");

        try
        {
            var response = await client.PostAsJsonAsync("api/auth/login", model);
            var content  = await response.Content.ReadAsStringAsync();
            var result   = JsonSerializer.Deserialize<AuthResponseDto>(content,
                               new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null || !result.Success)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Login failed.");
                return View(model);
            }

            // Store JWT in HTTP-only cookie
            Response.Cookies.Append("SmartBankToken", result.Token!, new CookieOptions
            {
                HttpOnly = true,
                Secure   = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires  = DateTimeOffset.UtcNow.AddDays(1)
            });
            Response.Cookies.Append("SmartBankRole", result.User?.RoleName ?? "Customer", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (string.Equals(result.User?.RoleName, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(result.User?.RoleName, "Manager", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Dashboard");
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            return View(model);
        }
    }

    // ─── GET /Auth/Register ───────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Register()
    {
        if (Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    // ─── POST /Auth/Register ──────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequestDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var client = _httpFactory.CreateClient("SmartBankAPI");

        try
        {
            var response = await client.PostAsJsonAsync("api/auth/register", model);
            var content  = await response.Content.ReadAsStringAsync();
            var result   = JsonSerializer.Deserialize<AuthResponseDto>(content,
                               new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null || !result.Success)
            {
                ModelState.AddModelError(string.Empty, result?.Message ?? "Registration failed.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Account created successfully! Please log in.";
            return RedirectToAction(nameof(Login));
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            return View(model);
        }
    }

    // ─── GET /Auth/Logout ─────────────────────────────────────────────────────
    public IActionResult Logout()
    {
        Response.Cookies.Delete("SmartBankToken");
        Response.Cookies.Delete("SmartBankRole");
        return RedirectToAction(nameof(Login));
    }
}
