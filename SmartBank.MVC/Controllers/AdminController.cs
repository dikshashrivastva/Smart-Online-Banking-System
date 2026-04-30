using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartBank.Models.DTOs.Service;

namespace SmartBank.MVC.Controllers;

public class AdminController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AdminController(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var dashboard = await GetEnvelopeDataAsync<AdminDashboardDto>(await client.GetAsync("api/admin/dashboard")) ?? new();
        return View(dashboard);
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var users = await GetEnvelopeDataAsync<List<AdminCustomerAccountDto>>(await client.GetAsync("api/admin/users")) ?? [];
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Freeze(int userId, bool freeze)
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        await client.PostAsJsonAsync("api/admin/freeze", new FreezeUserRequestDto { UserId = userId, Freeze = freeze });
        TempData["SuccessMessage"] = freeze ? "Customer frozen." : "Customer unfrozen.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateKyc(UpdateKycStatusRequestDto model)
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        await client.PostAsJsonAsync("api/admin/users/kyc", model);
        TempData["SuccessMessage"] = "KYC status updated.";
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AccountStatus(AccountStatusRequestDto model)
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        await client.PostAsJsonAsync("api/admin/accounts/status", model);
        TempData["SuccessMessage"] = $"Account marked {model.Status}.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Loans()
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var loans = await GetEnvelopeDataAsync<List<AdminLoanDto>>(await client.GetAsync("api/admin/loans")) ?? [];
        return View(loans);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoanDecision(LoanDecisionRequestDto model)
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        await client.PostAsJsonAsync("api/admin/loan/approve", model);
        TempData["SuccessMessage"] = model.Approve ? "Loan approved." : "Loan rejected.";
        return RedirectToAction(nameof(Loans));
    }

    [HttpGet]
    public async Task<IActionResult> Tickets()
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var tickets = await GetEnvelopeDataAsync<List<TicketDto>>(await client.GetAsync("api/admin/tickets")) ?? [];
        return View(tickets);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TicketReply(int ticketId, TicketUpdateRequestDto model)
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        await client.PostAsJsonAsync($"api/admin/tickets/{ticketId}/reply", model);
        TempData["SuccessMessage"] = "Ticket updated.";
        return RedirectToAction(nameof(Tickets));
    }

    [HttpGet]
    public async Task<IActionResult> Reports(int? userId)
    {
        var client = CreateAdminClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var url = userId.HasValue ? $"api/admin/reports?userId={userId}" : "api/admin/reports";
        var reports = await GetEnvelopeDataAsync<List<CustomerReportDto>>(await client.GetAsync(url)) ?? [];
        return View(reports);
    }

    private HttpClient? CreateAdminClient()
    {
        if (!Request.Cookies.TryGetValue("SmartBankToken", out var token))
            return null;

        if (!Request.Cookies.TryGetValue("SmartBankRole", out var role) ||
            (!role.Equals("Admin", StringComparison.OrdinalIgnoreCase) && !role.Equals("Manager", StringComparison.OrdinalIgnoreCase)))
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
}
