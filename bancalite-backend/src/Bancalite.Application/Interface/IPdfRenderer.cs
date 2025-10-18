namespace Bancalite.Application.Interface
{
    /// <summary>
    /// Servicio para renderizar PDF desde un DTO de reporte.
    /// </summary>
    public interface IPdfRenderer
    {
        /// <summary>
        /// Renderiza un reporte de estado de cuenta en PDF.
        /// </summary>
        Task<byte[]> RenderEstadoCuentaAsync(object reporteDto, CancellationToken ct = default);
    }
}

