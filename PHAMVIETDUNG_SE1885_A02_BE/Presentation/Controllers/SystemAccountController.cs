using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services;
using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.Presentation.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SystemAccountController : ControllerBase
  {
    private readonly ISystemAccountService _service;

    public SystemAccountController(ISystemAccountService service)
    {
      _service = service;
    }

    [EnableQuery(MaxTop = 100, PageSize = 20)]
    [HttpGet]
    public IActionResult Get()
    {
      return Ok(_service.GetAllAccounts().AsQueryable());
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
      var account = _service.GetAccountById(id);
      if (account == null) return NotFound();
      return Ok(account);
    }

    [HttpPost]
    public IActionResult Post([FromBody] SystemAccount account)
    {
      try
      {
        _service.CreateAccount(account);
        return CreatedAtAction(nameof(Get), new { id = account.AccountId }, account);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] SystemAccount account)
    {
      if (id != account.AccountId) return BadRequest("ID mismatch");
      try
      {
        _service.UpdateAccount(account);
        return Ok(account);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
      try
      {
        _service.DeleteAccount(id);
        return NoContent();
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
      var account = _service.Login(request.Email, request.Password);
      if (account == null) return Unauthorized();
      return Ok(account);
    }
  }

  public class LoginRequest
  {
    public required string Email { get; set; }
    public required string Password { get; set; }
  }
}
