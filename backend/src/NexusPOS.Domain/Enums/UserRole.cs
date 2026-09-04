namespace NexusPOS.Domain.Enums;

// Roles utilizados por JWT y por los atributos Authorize de la API.
public enum UserRole
{
    // Puede consultar el catálogo sin tener una cuenta persistida.
    Guest,
    // Puede comprar y consultar sus propias ventas y facturas.
    Customer,
    // Puede administrar catálogo, clientes, inventario, métricas y movimientos.
    Admin
}
