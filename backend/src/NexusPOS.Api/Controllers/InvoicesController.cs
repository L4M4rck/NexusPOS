using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Invoices;

namespace NexusPOS.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer,Admin")]
[Route("api/invoices")]
public sealed class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await invoiceService.GetInvoicesAsync(this.CurrentUserId(), this.IsAdmin(), cancellationToken));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<InvoiceResponse>> Get(long id, CancellationToken cancellationToken) =>
        Ok(await invoiceService.GetInvoiceAsync(id, this.CurrentUserId(), this.IsAdmin(), cancellationToken));
}
