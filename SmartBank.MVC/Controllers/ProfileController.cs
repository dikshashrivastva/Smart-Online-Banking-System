using Microsoft.AspNetCore.Mvc;
using SmartBank.Models.DTOs.Customer;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace SmartBank.MVC.Controllers;

public class ProfileController : Controller
{
    private readonly IHttpClientFactory _httpFactory;

    public ProfileController(IHttpClientFactory httpFactory)
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
            var profile = await client.GetFromJsonAsync<CustomerProfileDto>("api/profile");
            return View(profile);
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            return View(new CustomerProfileDto());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UpdateCustomerProfileDto model)
    {
        if (!ModelState.IsValid)
            return View(MapProfile(model));

        var client = CreateAuthorizedClient();
        if (client is null)
            return RedirectToAction("Login", "Auth");

        try
        {
            var response = await client.PutAsJsonAsync("api/profile", model);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Profile could not be updated.");
                return View(MapProfile(model));
            }
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Cannot reach the API server. Please try again.");
            return View(MapProfile(model));
        }

        TempData["SuccessMessage"] = "Profile updated successfully.";
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

    private static CustomerProfileDto MapProfile(UpdateCustomerProfileDto model) => new()
    {
        FirstName = model.FirstName,
        LastName = model.LastName,
        PhoneNumber = model.PhoneNumber,
        DateOfBirth = model.DateOfBirth,
        Gender = model.Gender,
        Address = model.Address,
        City = model.City,
        Country = model.Country,
        KycStatus = "Pending"
    };
}
