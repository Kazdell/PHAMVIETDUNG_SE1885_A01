using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A01_BE.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly FUNewsManagementContext _context;

        public AuditLogController(FUNewsManagementContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? userEmail = null,
            [FromQuery] string? entityType = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(userEmail))
            {
                query = query.Where(a => a.UserEmail.Contains(userEmail));
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.TableName == entityType);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= toDate.Value);
            }

            var totalCount = await query.CountAsync();
            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Data = logs
            });
        }

        [HttpGet("entities")]
        public async Task<IActionResult> GetEntityTypes()
        {
            var entities = await _context.AuditLogs
                .Select(a => a.TableName)
                .Distinct()
                .ToListAsync();
            return Ok(entities);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.AuditLogs
                .Select(a => a.UserEmail)
                .Distinct()
                .ToListAsync();
            return Ok(users);
        }
    }
}
