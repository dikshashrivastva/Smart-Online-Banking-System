using Microsoft.AspNetCore.Mvc;
using SmartBank.Models.DTOs.Customer;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SmartBank.MVC.Controllers;

public class AccountsController : Controller
{
    private readonly IHttpClientFactory _httpFactory;

    public AccountsController(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        try
        {
            var accounts = await client.GetFromJsonAsync<List<AccountDto>>("api/accounts") ?? [];
            return View(accounts);
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            return View(new List<AccountDto>());
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Login", "Auth");

        return View(new CreateAccountRequestDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountRequestDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        try
        {
            var response = await client.PostAsJsonAsync("api/accounts/create", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Account could not be opened.");
                return View(model);
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Account opened successfully.";
        return RedirectToAction(nameof(Index));
    }

    private HttpClient? CreateAuthorizedClient()
    {
        if (!Request.Cookies.TryGetValue("SmartBankToken", out var token))
            return null;

        var client = _httpFactory.CreateClient("SmartBankAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
