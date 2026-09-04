using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Admin;

namespace NexusPOS.Api.Controllers;
// Expone las métricas consolidadas utilizadas por el panel administrativo.
// Todos sus endpoints requieren un JWT con rol Admin.
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    // GET /api/admin/dashboard: calcula indicadores, series y rankings para el periodo solicitado.
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardResponse>> Dashboard([FromQuery] string period = "monthly", CancellationToken cancellationToken = default) =>
        Ok(await adminService.GetDashboardAsync(period, cancellationToken));
}
