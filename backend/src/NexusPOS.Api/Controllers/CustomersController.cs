using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Admin;

namespace NexusPOS.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/customers")]
public sealed class CustomersController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await adminService.GetCustomersAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> Get(int id, CancellationToken cancellationToken) =>
        Ok(await adminService.GetCustomerAsync(id, cancellationToken));
}
