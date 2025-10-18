using AutoMapper;
using Bancalite.Application.Core;
using Bancalite.Application.Cuentas.CuentaResponse;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.GetCuenta
{
    /// <summary>
    /// Query para obtener detalle de cuenta por Id.
    /// </summary>
    public class GetCuentaQuery
    {
        /// <summary>
        /// Solicitud con identificador de cuenta.
        /// </summary>
        /// <param name="Id">Id de la cuenta a consultar.</param>
        public record GetCuentaQueryRequest(Guid Id) : IRequest<Result<CuentaDto>>;

        internal class Handler : IRequestHandler<GetCuentaQueryRequest, Result<CuentaDto>>
        {
            private readonly BancaliteContext _context;
            private readonly IMapper _mapeador;

            /// <summary>
            /// Crea un handler para obtener detalle de cuenta.
            /// </summary>
            /// <param name="context">Contexto de datos.</param>
            /// <param name="mapeador">AutoMapper para proyecciones.</param>
            public Handler(BancaliteContext context, IMapper mapeador)
            {
                _context = context;
                _mapeador = mapeador;
            }

            /// <summary>
            /// Ejecuta la consulta por Id y retorna el DTO.
            /// </summary>
            /// <param name="request">Request con el Id de cuenta.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>CuentaDto o error si no existe.</returns>
            public async Task<Result<CuentaDto>> Handle(GetCuentaQueryRequest request, CancellationToken cancellationToken)
            {
                // Cargar cuenta con tipo y titular
                var c = await _context.Cuentas
                    .AsNoTracking()
                    .Include(x => x.TipoCuenta)
                    .Include(x => x.Cliente).ThenInclude(cl => cl.Persona)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                if (c == null) return Result<CuentaDto>.Failure("No encontrado");

                // Mapear a DTO
                var dto = new CuentaDto
                {
                    CuentaId = c.Id,
                    NumeroCuenta = c.NumeroCuenta,
                    TipoCuentaId = c.TipoCuentaId,
                    TipoCuentaNombre = c.TipoCuenta.Nombre,
                    ClienteId = c.ClienteId,
                    ClienteNombre = $"{c.Cliente.Persona.Nombres} {c.Cliente.Persona.Apellidos}",
                    SaldoInicial = c.SaldoInicial,
                    SaldoActual = c.SaldoActual,
                    Estado = c.Estado.ToString(),
                    FechaApertura = c.FechaApertura,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                };
                return Result<CuentaDto>.Success(dto);
            }
        }
    }
}
