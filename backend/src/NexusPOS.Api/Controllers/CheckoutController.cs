using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusPOS.Application.Checkout;

namespace NexusPOS.Api.Controllers;
// Punto de entrada del proceso de compra. El cliente de la venta se obtiene
// del JWT y nunca se confía en un identificador enviado por el navegador.
[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/checkout")]
public sealed class CheckoutController(ICheckoutService checkoutService) : ControllerBase
{
    // POST /api/checkout: valida y confirma una venta, descuenta stock,
    // procesa el pago simulado y genera la factura.
    [HttpPost]
    public async Task<ActionResult<SaleResponse>> Checkout(CheckoutRequest request, CancellationToken cancellationToken)
    {
        // CurrentUserId extrae la identidad autenticada; así la venta queda ligada
        // automáticamente al cliente que inició sesión.
        var sale = await checkoutService.CheckoutAsync(this.CurrentUserId(), request, cancellationToken);
        return Ok(sale);
    }
}
