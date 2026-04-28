using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.Services.Interfaces;
using SmartBank.Models.DTOs.Customer;
using System.Security.Claims;

namespace SmartBank.API.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public AccountsController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _customerService.GetAccountsAsync(GetUserId()));

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateAccountRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var account = await _customerService.CreateAccountAsync(GetUserId(), request);
        return CreatedAtAction(nameof(Get), new { id = account.AccountId }, account);
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}
