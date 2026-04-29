using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartBank.Models.DTOs.Customer;
using SmartBank.Models.DTOs.Transactions;

namespace SmartBank.MVC.Controllers;

public class TransactionsController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TransactionsController(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Deposit()
        => await TransactionFormView("Deposit");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(TransactionFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(await WithAccounts(model));

        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var result = await PostForTransactionAsync(client, "api/transactions/deposit", new DepositRequestDto
        {
            AccountId = model.AccountId,
            Amount = model.Amount,
            Description = model.Description
        });

        if (result is null)
            return View(await WithAccounts(model));

        TempData["SuccessMessage"] = "Deposit completed successfully.";
        return RedirectToAction(nameof(Receipt), new { id = result.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Withdraw()
        => await TransactionFormView("Withdraw");

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(TransactionFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(await WithAccounts(model));

        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var result = await PostForTransactionAsync(client, "api/transactions/withdraw", new WithdrawRequestDto
        {
            AccountId = model.AccountId,
            Amount = model.Amount,
            Description = model.Description
        });

        if (result is null)
            return View(await WithAccounts(model));

        TempData["SuccessMessage"] = "Withdrawal completed successfully.";
        return RedirectToAction(nameof(Receipt), new { id = result.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Transfer()
    {
        if (!Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Login", "Auth");

        return View(new TransferFormViewModel { Accounts = await LoadAccountsAsync() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(TransferFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(await WithAccounts(model));

        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var lookup = await GetEnvelopeDataAsync<ToAccountLookupDto>(await client.GetAsync($"api/accounts/lookup?accountNumber={Uri.EscapeDataString(model.ToAccountNumber)}"));
        if (lookup is null)
        {
            ModelState.AddModelError(nameof(model.ToAccountNumber), "Destination account was not found.");
            return View(await WithAccounts(model));
        }

        client.DefaultRequestHeaders.Remove("X-Idempotency-Key");
        client.DefaultRequestHeaders.Add("X-Idempotency-Key", Guid.NewGuid().ToString("N"));

        var result = await PostForTransactionAsync(client, "api/transactions/transfer", new TransferRequestDto
        {
            FromAccountId = model.FromAccountId,
            ToAccountId = lookup.AccountId,
            Amount = model.Amount,
            Description = model.Description
        });

        if (result is null)
            return View(await WithAccounts(model));

        TempData["SuccessMessage"] = "Transfer completed successfully.";
        return RedirectToAction(nameof(Receipt), new { id = result.Id });
    }

    [HttpGet]
    public async Task<IActionResult> History(TransactionHistoryViewModel model)
    {
        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        model.Accounts = await LoadAccountsAsync();
        if (model.AccountId == 0)
            model.AccountId = model.Accounts.FirstOrDefault()?.AccountId ?? 0;

        if (model.AccountId != 0)
        {
            var query = $"api/transactions/history?accountId={model.AccountId}&page={Math.Max(1, model.Page)}&size={Math.Clamp(model.Size, 1, 100)}";
            if (model.From.HasValue)
                query += $"&from={model.From:yyyy-MM-dd}";
            if (model.To.HasValue)
                query += $"&to={model.To:yyyy-MM-dd}";
            if (!string.IsNullOrWhiteSpace(model.Type))
                query += $"&type={Uri.EscapeDataString(model.Type)}";

            model.Results = await GetEnvelopeDataAsync<PagedResultDto<TransactionResponseDto>>(await client.GetAsync(query)) ?? new();
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Receipt(int id)
    {
        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var receipt = await GetEnvelopeDataAsync<TransactionReceiptDto>(await client.GetAsync($"api/transactions/{id}/receipt"));
        return receipt is null ? RedirectToAction(nameof(History)) : View(receipt);
    }

    [HttpGet]
    public async Task<IActionResult> Statement(int accountId, DateTime? from, DateTime? to)
    {
        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var query = $"api/transactions/statement?accountId={accountId}";
        if (from.HasValue)
            query += $"&from={from:yyyy-MM-dd}";
        if (to.HasValue)
            query += $"&to={to:yyyy-MM-dd}";

        var response = await client.GetAsync(query);
        if (!response.IsSuccessStatusCode)
            return RedirectToAction(nameof(History), new { accountId, from, to });

        var bytes = await response.Content.ReadAsByteArrayAsync();
        return File(bytes, "text/csv", $"statement-{accountId}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> Balance(int accountId)
    {
        var accounts = await LoadAccountsAsync();
        var account = accounts.FirstOrDefault(a => a.AccountId == accountId);
        return Json(new { balance = account?.Balance ?? 0m, currency = account?.Currency ?? "INR" });
    }

    private async Task<IActionResult> TransactionFormView(string viewName)
    {
        if (!Request.Cookies.ContainsKey("SmartBankToken"))
            return RedirectToAction("Login", "Auth");

        return View(viewName, new TransactionFormViewModel { Accounts = await LoadAccountsAsync() });
    }

    private async Task<TransactionFormViewModel> WithAccounts(TransactionFormViewModel model)
    {
        model.Accounts = await LoadAccountsAsync();
        return model;
    }

    private async Task<TransferFormViewModel> WithAccounts(TransferFormViewModel model)
    {
        model.Accounts = await LoadAccountsAsync();
        return model;
    }

    private async Task<TransactionResponseDto?> PostForTransactionAsync<T>(HttpClient client, string url, T payload)
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await ReadApiMessageAsync(response));
                return null;
            }

            return await GetEnvelopeDataAsync<TransactionResponseDto>(response);
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            return null;
        }
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
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind == JsonValueKind.Null)
            return default;

        return data.Deserialize<T>(JsonOptions);
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
