using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartBank.Models.DTOs.Customer;
using SmartBank.Models.DTOs.Service;

namespace SmartBank.MVC.Controllers;

public class LoansController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LoansController(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Apply()
    {
        if (!Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Login", "Auth");

        ViewBag.Accounts = await LoadAccountsAsync();
        return View(new LoanApplyRequestDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(LoanApplyRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Accounts = await LoadAccountsAsync();
            return View(model);
        }

        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        try
        {
            var response = await client.PostAsJsonAsync("api/loans/apply", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await ReadApiMessageAsync(response));
                ViewBag.Accounts = await LoadAccountsAsync();
                return View(model);
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            ViewBag.Accounts = await LoadAccountsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Loan application submitted.";
        return RedirectToAction(nameof(Status));
    }

    [HttpGet]
    public async Task<IActionResult> Status()
    {
        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var loans = await GetEnvelopeDataAsync<List<LoanStatusDto>>(await client.GetAsync("api/loans/status")) ?? [];
        return View(loans);
    }

    private async Task<List<AccountDto>> LoadAccountsAsync()
    {
        var client = CreateAuthorizedClient();
        if (client is null)
            return [];

        try
        {
            return await client.GetFromJsonAsync<List<AccountDto>>("api/accounts") ?? [];
        }
        catch
        {
            return [];
        }
    }

    private HttpClient? CreateAuthorizedClient()
    {
        if (!Request.Cookies.TryGetValue("SmartBankToken", out var token))
            return null;

        var client = _httpFactory.CreateClient("SmartBankAPI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<T?> GetEnvelopeDataAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
            return default;

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);
        return document.RootElement.TryGetProperty("data", out var data) ? data.Deserialize<T>(JsonOptions) : default;
    }

    private static async Task<string> ReadApiMessageAsync(HttpResponseMessage response)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            return document.RootElement.TryGetProperty("message", out var message)
                ? message.GetString() ?? "Request failed."
                : "Request failed.";
        }
        catch
        {
            return "Request failed.";
        }
    }
}
