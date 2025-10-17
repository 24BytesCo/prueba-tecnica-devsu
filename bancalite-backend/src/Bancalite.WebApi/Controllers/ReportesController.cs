using System;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Interface;
using Bancalite.Application.Reportes.EstadoCuenta;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bancalite.WebApi.Controllers
{
    /// <summary>
    /// Controlador de reportes: estado de cuenta (JSON y PDF).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly IPdfRenderer _pdf;
        public ReportesController(ISender sender, IPdfRenderer pdf)
        {
            _sender = sender;
            _pdf = pdf;
        }

        /// <summary>
        /// Devuelve el reporte de estado de cuenta (JSON).
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<EstadoCuentaDto>> Get([FromQuery] Guid? clienteId, [FromQuery] string? numeroCuenta, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        {
            var req = new EstadoCuentaRequest { ClienteId = clienteId, NumeroCuenta = numeroCuenta, Desde = desde, Hasta = hasta };
            var result = await _sender.Send(new EstadoCuentaQuery.EstadoCuentaQueryRequest(req), ct);
            if (!result.IsSuccess) return Problem(statusCode: StatusCodeFromError(result.Error), title: result.Error);
            return Ok(result.Datos);
        }

        /// <summary>
        /// Devuelve el reporte de estado de cuenta como PDF.
        /// </summary>
        [HttpGet("pdf")]
        [Authorize]
        public async Task<IActionResult> GetPdf([FromQuery] Guid? clienteId, [FromQuery] string? numeroCuenta, [FromQuery] DateTime desde, [FromQuery] DateTime hasta, CancellationToken ct)
        {
            var req = new EstadoCuentaRequest { ClienteId = clienteId, NumeroCuenta = numeroCuenta, Desde = desde, Hasta = hasta };
            var result = await _sender.Send(new EstadoCuentaQuery.EstadoCuentaQueryRequest(req), ct);
            if (!result.IsSuccess) return Problem(statusCode: StatusCodeFromError(result.Error), title: result.Error);

            var bytes = await _pdf.RenderEstadoCuentaAsync(result.Datos!, ct);
            var fileName = $"reporte-estado-cuenta-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        private static int StatusCodeFromError(string? error)
        {
            var e = error ?? string.Empty;
            if (e.StartsWith("Unauthorized", StringComparison.OrdinalIgnoreCase)) return 401;
            if (e.StartsWith("Forbidden", StringComparison.OrdinalIgnoreCase)) return 403;
            if (e.StartsWith("BadRequest", StringComparison.OrdinalIgnoreCase)) return 400;
            if (e.StartsWith("No encontrado", StringComparison.OrdinalIgnoreCase)) return 404;
            if (e.StartsWith("Unprocessable", StringComparison.OrdinalIgnoreCase)) return 422;
            if (e.StartsWith("Conflict", StringComparison.OrdinalIgnoreCase)) return 409;
            return 400;
        }
    }
}

