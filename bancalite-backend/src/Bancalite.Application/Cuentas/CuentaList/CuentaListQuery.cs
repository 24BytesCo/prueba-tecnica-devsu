using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Cuentas.CuentaList
{
    /// <summary>
    /// Query para listar cuentas con paginación y filtros.
    /// </summary>
    public class CuentaListQuery
    {
        /// <summary>
        /// Filtros de consulta.
        /// </summary>
        /// <param name="Pagina">Número de página (1-based).</param>
        /// <param name="Tamano">Tamaño de página.</param>
        /// <param name="ClienteId">Filtra por cliente.</param>
        /// <param name="Estado">Filtra por estado (Activa/Inactiva/Bloqueada).</param>
        public record CuentaListQueryRequest(
            int Pagina = 1,
            int Tamano = 10,
            Guid? ClienteId = null,
            string? Estado = null
        ) : IRequest<Result<Paged<CuentaListItem>>>;

        internal class Handler : IRequestHandler<CuentaListQueryRequest, Result<Paged<CuentaListItem>>>
        {
            private readonly BancaliteContext _context;
            private readonly IMapper _mapeador;

            /// <summary>
            /// Crea un handler para listar cuentas.
            /// </summary>
            /// <param name="context">Contexto de datos.</param>
            /// <param name="mapeador">AutoMapper para proyectar DTOs.</param>
            public Handler(BancaliteContext context, IMapper mapeador)
            {
                _context = context;
                _mapeador = mapeador;
            }

            /// <summary>
            /// Ejecuta el listado con filtros y paginación.
            /// </summary>
            /// <param name="request">Parámetros de consulta (página, tamaño, filtros).</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado paginado de cuentas.</returns>
            public async Task<Result<Paged<CuentaListItem>>> Handle(CuentaListQueryRequest request, CancellationToken cancellationToken)
            {
                // Query base con includes necesarios
                var query = _context.Cuentas
                    .AsNoTracking()
                    .Include(c => c.TipoCuenta)
                    .Include(c => c.Cliente).ThenInclude(cl => cl.Persona)
                    .AsQueryable();

                // Filtro por cliente
                if (request.ClienteId.HasValue && request.ClienteId.Value != Guid.Empty)
                {
                    query = query.Where(c => c.ClienteId == request.ClienteId.Value);
                }

                // Filtro por estado (string contra enum ToString)
                if (!string.IsNullOrWhiteSpace(request.Estado))
                {
                    var estado = request.Estado.Trim();
                    query = query.Where(c => c.Estado.ToString().Equals(estado, StringComparison.OrdinalIgnoreCase));
                }

                // Total para paginación
                var total = await query.CountAsync(cancellationToken);

                // Orden estable por número de cuenta
                query = query.OrderBy(c => c.NumeroCuenta);
                var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
                var tamano = request.Tamano <= 0 ? 10 : request.Tamano;
                var skip = (pagina - 1) * tamano;

                // Cargar entidades y mapear a DTOs livianos
                var entidades = await query.Skip(skip).Take(tamano).ToListAsync(cancellationToken);
                var items = entidades.Select(c => new CuentaListItem
                {
                    CuentaId = c.Id,
                    NumeroCuenta = c.NumeroCuenta,
                    TipoCuentaId = c.TipoCuentaId,
                    TipoCuentaNombre = c.TipoCuenta.Nombre,
                    ClienteId = c.ClienteId,
                    ClienteNombre = $"{c.Cliente.Persona.Nombres} {c.Cliente.Persona.Apellidos}",
                    SaldoActual = c.SaldoActual,
                    Estado = c.Estado.ToString(),
                    FechaApertura = c.FechaApertura
                }).ToList();

                // Empaquetar paginado
                var paged = new Paged<CuentaListItem>
                {
                    Items = items,
                    Total = total,
                    Pagina = pagina,
                    Tamano = tamano
                };
                return Result<Paged<CuentaListItem>>.Success(paged);
            }
        }
    }
}
