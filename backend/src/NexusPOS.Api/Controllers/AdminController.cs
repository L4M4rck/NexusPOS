using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Admin;

namespace NexusPOS.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> Dashboard([FromQuery] string period = "monthly", CancellationToken cancellationToken = default) =>
        Ok(await adminService.GetDashboardAsync(period, cancellationToken));
}
