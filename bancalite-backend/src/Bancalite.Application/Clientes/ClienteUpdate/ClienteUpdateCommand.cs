using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using Bancalite.Persitence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Bancalite.Application.Interface;

namespace Bancalite.Application.Clientes.ClienteUpdate
{
    /// <summary>
    /// Comandos para actualizar clientes (PUT y PATCH).
    /// </summary>
    public class ClienteUpdateCommand
    {
        /// <summary>
        /// PUT: actualización total. Reemplaza todos los campos declarados.
        /// </summary>
        /// <param name="Id">Identificador del cliente a actualizar.</param>
        /// <param name="Request">Contenido a aplicar en la actualización total.</param>
        public record ClientePutCommandRequest(Guid Id, ClientePutRequest Request) : IRequest<Result<bool>>;

        /// <summary>
        /// PATCH: actualización parcial. Solo aplica campos presentes.
        /// </summary>
        /// <param name="Id">Identificador del cliente a actualizar parcialmente.</param>
        /// <param name="Request">Campos parciales a aplicar.</param>
        public record ClientePatchCommandRequest(Guid Id, ClientePatchRequest Request) : IRequest<Result<bool>>;

        internal class PutHandler : IRequestHandler<ClientePutCommandRequest, Result<bool>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;

            public PutHandler(BancaliteContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            /// <summary>
            /// Aplica actualización total al cliente.
            /// </summary>
            /// <param name="request">Id y datos completos del cliente.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado indicando éxito o error.</returns>
            public async Task<Result<bool>> Handle(ClientePutCommandRequest request, CancellationToken cancellationToken)
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Persona)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                if (cliente == null)
                {
                    return Result<bool>.Failure("Cliente no encontrado");
                }

                // Autorización: si no es Admin, sólo su propio cliente
                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad)) return Result<bool>.Failure("Unauthorized");
                var esAdmin = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     join u in _context.Users on ur.UserId equals u.Id
                                     where (u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad) && r.Name == "Admin"
                                     select ur).AnyAsync(cancellationToken);
                if (!esAdmin)
                {
                    var userId = await _context.Users.AsNoTracking()
                        .Where(u => u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad)
                        .Select(u => u.Id).FirstOrDefaultAsync(cancellationToken);
                    if (cliente.AppUserId == null || cliente.AppUserId != userId)
                        return Result<bool>.Failure("Forbidden");
                }

                // Validar unicidad de documento si cambió
                if (cliente.Persona.TipoDocumentoIdentidadId != request.Request.TipoDocumentoIdentidadId
                    || !string.Equals(cliente.Persona.NumeroDocumento, request.Request.NumeroDocumento, StringComparison.Ordinal))
                {
                    var duplicado = await _context.Personas.AsNoTracking().AnyAsync(p =>
                        p.Id != cliente.PersonaId &&
                        p.TipoDocumentoIdentidadId == request.Request.TipoDocumentoIdentidadId &&
                        p.NumeroDocumento == request.Request.NumeroDocumento,
                        cancellationToken);
                    if (duplicado)
                    {
                        return Result<bool>.Failure("La persona con el documento indicado ya existe");
                    }
                }

                // Aplicar cambios
                var p = cliente.Persona;
                p.Nombres = request.Request.Nombres.Trim();
                p.Apellidos = request.Request.Apellidos.Trim();
                p.Edad = request.Request.Edad;
                p.GeneroId = request.Request.GeneroId;
                p.TipoDocumentoIdentidadId = request.Request.TipoDocumentoIdentidadId;
                p.NumeroDocumento = request.Request.NumeroDocumento.Trim();
                p.Direccion = string.IsNullOrWhiteSpace(request.Request.Direccion) ? null : request.Request.Direccion.Trim();
                p.Telefono = string.IsNullOrWhiteSpace(request.Request.Telefono) ? null : request.Request.Telefono.Trim();
                p.Email = string.IsNullOrWhiteSpace(request.Request.Email) ? null : request.Request.Email.Trim();

                bool inhabilitar = request.Request.Estado == false && cliente.Estado == true;
                cliente.Estado = request.Request.Estado;

                // Si se inhabilitó el cliente, inhabilitar todas sus cuentas
                if (inhabilitar)
                {
                    var cuentas = await _context.Cuentas.Where(c => c.ClienteId == cliente.Id).ToListAsync(cancellationToken);
                    foreach (var cta in cuentas)
                    {
                        if (cta.Estado != Domain.EstadoCuenta.Inactiva)
                            cta.Desactivar();
                    }
                }

                cliente.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }

        internal class PatchHandler : IRequestHandler<ClientePatchCommandRequest, Result<bool>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;

            public PatchHandler(BancaliteContext context, IUserAccessor userAccessor)
            {
                _context = context;
                _userAccessor = userAccessor;
            }

            /// <summary>
            /// Aplica actualización parcial al cliente.
            /// </summary>
            /// <param name="request">Id y campos parciales a actualizar.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Resultado indicando éxito o error.</returns>
            public async Task<Result<bool>> Handle(ClientePatchCommandRequest request, CancellationToken cancellationToken)
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Persona)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                if (cliente == null)
                {
                    return Result<bool>.Failure("Cliente no encontrado");
                }

                // Autorización: si no es Admin, sólo su propio cliente
                var identidad = _userAccessor.GetUsername();
                if (string.IsNullOrWhiteSpace(identidad)) return Result<bool>.Failure("Unauthorized");
                var esAdmin = await (from ur in _context.UserRoles
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     join u in _context.Users on ur.UserId equals u.Id
                                     where (u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad) && r.Name == "Admin"
                                     select ur).AnyAsync(cancellationToken);
                if (!esAdmin)
                {
                    var userId = await _context.Users.AsNoTracking()
                        .Where(u => u.Email == identidad || u.UserName == identidad || u.Id.ToString() == identidad)
                        .Select(u => u.Id).FirstOrDefaultAsync(cancellationToken);
                    if (cliente.AppUserId == null || cliente.AppUserId != userId)
                        return Result<bool>.Failure("Forbidden");
                }

                var p = cliente.Persona;

                // Cambios condicionales
                if (!string.IsNullOrWhiteSpace(request.Request.Nombres)) p.Nombres = request.Request.Nombres.Trim();
                if (!string.IsNullOrWhiteSpace(request.Request.Apellidos)) p.Apellidos = request.Request.Apellidos.Trim();
                if (request.Request.Edad.HasValue) p.Edad = request.Request.Edad.Value;
                if (request.Request.GeneroId.HasValue) p.GeneroId = request.Request.GeneroId.Value;
                if (request.Request.TipoDocumentoIdentidadId.HasValue) p.TipoDocumentoIdentidadId = request.Request.TipoDocumentoIdentidadId.Value;
                if (!string.IsNullOrWhiteSpace(request.Request.NumeroDocumento)) p.NumeroDocumento = request.Request.NumeroDocumento.Trim();
                if (request.Request.Direccion != null) p.Direccion = string.IsNullOrWhiteSpace(request.Request.Direccion) ? null : request.Request.Direccion.Trim();
                if (request.Request.Telefono != null) p.Telefono = string.IsNullOrWhiteSpace(request.Request.Telefono) ? null : request.Request.Telefono.Trim();
                if (request.Request.Email != null) p.Email = string.IsNullOrWhiteSpace(request.Request.Email) ? null : request.Request.Email.Trim();
                bool inhabilitar = request.Request.Estado.HasValue && request.Request.Estado.Value == false && cliente.Estado == true;
                if (request.Request.Estado.HasValue) cliente.Estado = request.Request.Estado.Value;

                // Validar unicidad de documento si cambió (si ambos campos están presentes)
                if (request.Request.TipoDocumentoIdentidadId.HasValue || !string.IsNullOrWhiteSpace(request.Request.NumeroDocumento))
                {
                    var tipoId = request.Request.TipoDocumentoIdentidadId ?? p.TipoDocumentoIdentidadId;
                    var numDoc = !string.IsNullOrWhiteSpace(request.Request.NumeroDocumento) ? request.Request.NumeroDocumento!.Trim() : p.NumeroDocumento;
                    var duplicado = await _context.Personas.AsNoTracking().AnyAsync(x =>
                        x.Id != p.Id && x.TipoDocumentoIdentidadId == tipoId && x.NumeroDocumento == numDoc,
                        cancellationToken);
                    if (duplicado)
                    {
                        return Result<bool>.Failure("La persona con el documento indicado ya existe");
                    }
                }

                // Si se inhabilitó el cliente, inhabilitar todas sus cuentas
                if (inhabilitar)
                {
                    var cuentas = await _context.Cuentas.Where(c => c.ClienteId == cliente.Id).ToListAsync(cancellationToken);
                    foreach (var cta in cuentas)
                    {
                        if (cta.Estado != Domain.EstadoCuenta.Inactiva)
                            cta.Desactivar();
                    }
                }

                cliente.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }
        }
    }
}
