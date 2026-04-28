using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBank.API.Services.Interfaces;
using SmartBank.Models.DTOs.Customer;
using System.Security.Claims;

namespace SmartBank.API.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public ProfileController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var profile = await _customerService.GetProfileAsync(GetUserId());
        return profile is null ? NotFound(new { Success = false, Message = "User not found." }) : Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateCustomerProfileDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var profile = await _customerService.UpdateProfileAsync(GetUserId(), request);
        return profile is null ? NotFound(new { Success = false, Message = "User not found." }) : Ok(profile);
    }

    private int GetUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
}
