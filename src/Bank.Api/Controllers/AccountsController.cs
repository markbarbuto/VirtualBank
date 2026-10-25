using Microsoft.AspNetCore.Mvc;

namespace Bank.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IGrainFactory grains) : ControllerBase
{
    [HttpGet("{id}")]
    public Task<AccountInfo> Get(string id) => 
        grains.GetGrain<IAccountGrain>(id).GetInfo();

    [HttpPost("{id}/deposit")]
    public Task<decimal> Deposit(string id, [FromBody] decimal amount) =>
        grains.GetGrain<IAccountGrain>(id).Deposit(amount);

    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(string id, [FromBody] decimal amount)
    {
        try
        {
            return Ok(await grains.GetGrain<IAccountGrain>(id).Withdraw(amount));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/transfer/{to}")]
    public async Task<IActionResult> Transfer(string id, string to, [FromBody] decimal amount)
    {
        try
        {
            await grains.GetGrain<IAccountGrain>(id).Transfer(to, amount);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

    }
}
