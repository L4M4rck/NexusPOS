using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Admin;

namespace NexusPOS.Api.Controllers;
// Permite al administrador consultar los perfiles de clientes registrados.
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/customers")]
public sealed class CustomersController(IAdminService adminService) : ControllerBase
{
    // GET /api/customers: devuelve todos los perfiles para la vista administrativa.
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await adminService.GetCustomersAsync(cancellationToken));
    // GET /api/customers/{id}: devuelve un cliente específico o responde 404.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> Get(int id, CancellationToken cancellationToken) =>
        Ok(await adminService.GetCustomerAsync(id, cancellationToken));
}
