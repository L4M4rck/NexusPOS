namespace NexusPOS.Application.Common;
// Respuesta genérica para listados paginados. Además de los elementos incluye
// metadatos para que el frontend pueda habilitar sus controles de navegación.
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages)
{
    // Indica si existe una página posterior.
    public bool HasNextPage => Page < TotalPages;

    // Indica si el usuario puede regresar a una página anterior.
    public bool HasPreviousPage => Page > 1;
}
