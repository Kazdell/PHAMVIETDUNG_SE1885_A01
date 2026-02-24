using Microsoft.AspNetCore.Mvc;
using PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services;
using System.Security.Claims;

namespace PHAMVIETDUNG_SE1885_A02_BE.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class NotificationController : ControllerBase
  {
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
      _notificationService = notificationService;
    }

    private short GetCurrentUserId()
    {
      var identity = HttpContext.User.Identity as ClaimsIdentity;
      if (identity != null)
      {
        var accountIdClaim = identity.FindFirst("AccountId");
        if (accountIdClaim != null && short.TryParse(accountIdClaim.Value, out short accountId))
        {
          return accountId;
        }
      }
      return 0; // Or throw unauthorized
    }

    [HttpGet("Me")]
    public async Task<IActionResult> GetMyNotifications([FromQuery] short? userId = null)
    {
      // Try JWT claim first, then query param fallback for cross-origin calls
      short accountId = GetCurrentUserId();
      if (accountId == 0 && userId.HasValue)
      {
        accountId = userId.Value;
      }
      if (accountId == 0) return Unauthorized();

      var notes = await _notificationService.GetUserNotificationsAsync(accountId, 10);
      return Ok(notes);
    }

    [HttpPost("MarkRead/{id}")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
      await _notificationService.MarkAsReadAsync(id);
      return Ok();
    }

    [HttpPost("MarkAllRead")]
    public async Task<IActionResult> MarkAllAsRead()
    {
      var userId = GetCurrentUserId();
      if (userId == 0) return Unauthorized();

      await _notificationService.MarkAllAsReadAsync(userId);
      return Ok();
    }
  }
}
