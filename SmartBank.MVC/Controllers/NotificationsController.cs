using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SmartBank.Models.DTOs.Service;

namespace SmartBank.MVC.Controllers;

public class NotificationsController : Controller
{
    private readonly IHttpClientFactory _httpFactory;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public NotificationsController(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        var notifications = await GetEnvelopeDataAsync<List<NotificationDto>>(await client.GetAsync("api/notifications")) ?? [];
        return View(notifications);
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
}
