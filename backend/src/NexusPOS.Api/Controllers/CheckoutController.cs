using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Checkout;

namespace NexusPOS.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/checkout")]
public sealed class CheckoutController(ICheckoutService checkoutService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Checkout(CheckoutRequest request, CancellationToken cancellationToken)
    {
        var sale = await checkoutService.CheckoutAsync(this.CurrentUserId(), request, cancellationToken);
        return Ok(sale);
    }
}
