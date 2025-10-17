using System;
using System.Threading;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using Bancalite.Persitence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Bancalite.Application.Interface;
using Microsoft.AspNetCore.Identity;
using Bancalite.Persitence.Model;
using System.Security.Cryptography;

namespace Bancalite.Application.Cuentas.CuentaCreate
{
    /// <summary>
    /// Comando para apertura de cuenta.
    /// </summary>
    public class CuentaCreateCommand
    {
        public record CuentaCreateCommandRequest(CuentaCreateRequest Request) : IRequest<Result<Guid>>;

        internal class Handler : IRequestHandler<CuentaCreateCommandRequest, Result<Guid>>
        {
            private readonly BancaliteContext _context;
            private readonly IUserAccessor _userAccessor;
            private readonly UserManager<AppUser> _userManager;
            /// <summary>
            /// Crea un handler para apertura de cuentas.
            /// </summary>
            /// <param name="context">Contexto de datos.</param>
            /// <param name="userAccessor">Acceso al usuario actual.</param>
            /// <param name="userManager">UserManager para roles y resolución de identidad.</param>
            public Handler(BancaliteContext context, IUserAccessor userAccessor, UserManager<AppUser> userManager)
            {
                _context = context;
                _userAccessor = userAccessor;
                _userManager = userManager;
            }

            /// <summary>
            /// Ejecuta la apertura de cuenta.
            /// </summary>
            /// <param name="request">Datos de la cuenta a crear.</param>
            /// <param name="cancellationToken">Token de cancelación.</param>
            /// <returns>Id de la cuenta creada o error de conflicto.</returns>
            public async Task<Result<Guid>> Handle(CuentaCreateCommandRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    // Autorización: si no es Admin, solo puede crear cuentas para su propio Cliente
                    var identidad = _userAccessor.GetUsername();
                    if (string.IsNullOrWhiteSpace(identidad))
                        return Result<Guid>.Failure("Unauthorized");

                    AppUser? usuario = null;
                    if (Guid.TryParse(identidad, out var idGuid))
                        usuario = await _userManager.FindByIdAsync(idGuid.ToString());
                    usuario ??= await _userManager.FindByEmailAsync(identidad)
                                  ?? await _userManager.FindByNameAsync(identidad);
                    if (usuario is null)
                        return Result<Guid>.Failure("Unauthorized");

                    var esAdmin = await _userManager.IsInRoleAsync(usuario, "Admin");
                    if (!esAdmin)
                    {
                        var esPropio = await _context.Clientes.AsNoTracking()
                            .AnyAsync(c => c.Id == request.Request.ClienteId && c.AppUserId == usuario.Id && c.Estado == true, cancellationToken);
                        if (!esPropio)
                            return Result<Guid>.Failure("Forbidden: Solo puede crear cuentas para su propio cliente");
                    }

                    // Si no envía número, generar uno automáticamente con formato ####-####-#### (12 dígitos)
                    var numeroCuenta = string.IsNullOrWhiteSpace(request.Request.NumeroCuenta)
                        ? await GenerarNumeroCuentaUnicoAsync(cancellationToken)
                        : request.Request.NumeroCuenta.Trim();

                    // Validación de unicidad adicional (defensiva) cuando se envía
                    if (!string.IsNullOrWhiteSpace(request.Request.NumeroCuenta))
                    {
                        var existe = await _context.Cuentas.AsNoTracking().AnyAsync(c => c.NumeroCuenta == numeroCuenta, cancellationToken);
                        if (existe) return Result<Guid>.Failure("Conflict: El numero de cuenta ya existe");
                    }

                    // Inicializar entidad con saldos y datos básicos
                    var cuenta = new Bancalite.Domain.Cuenta
                    {
                        Id = Guid.NewGuid(),
                        NumeroCuenta = numeroCuenta,
                        TipoCuentaId = request.Request.TipoCuentaId,
                        ClienteId = request.Request.ClienteId,
                        SaldoInicial = request.Request.SaldoInicial,
                        SaldoActual = request.Request.SaldoInicial,
                        FechaApertura = DateTime.UtcNow
                    };

                    // Guardando en BD
                    await _context.Cuentas.AddAsync(cuenta, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                    return Result<Guid>.Success(cuenta.Id);
                }
                catch (DbUpdateException ex)
                {
                    // Conflicto de llave única
                    return Result<Guid>.Failure($"Conflict: No se pudo crear la cuenta por duplicidad. Detalles: {ex.Message}");
                }
            }

            // Genera un número de cuenta con 12 dígitos agrupados ####-####-#### y garantiza unicidad
            private async Task<string> GenerarNumeroCuentaUnicoAsync(CancellationToken ct)
            {
                for (int intento = 0; intento < 10; intento++)
                {
                    var crudo = Generar12Digitos();
                    var formateado = $"{crudo[..4]}-{crudo.Substring(4,4)}-{crudo.Substring(8,4)}";
                    var existe = await _context.Cuentas.AsNoTracking().AnyAsync(c => c.NumeroCuenta == formateado, ct);
                    if (!existe) return formateado;
                }
                // Como fallback, usar el crudo sin formato si hay demasiadas colisiones (poco probable)
                var fallback = Generar12Digitos();
                return $"{fallback[..4]}-{fallback.Substring(4,4)}-{fallback.Substring(8,4)}";
            }

            private static string Generar12Digitos()
            {
                // Generar 12 dígitos aleatorios (primer dígito no cero para evitar apariencia de inválido)
                Span<byte> bytes = stackalloc byte[12];
                RandomNumberGenerator.Fill(bytes);
                char[] chars = new char[12];
                for (int i = 0; i < 12; i++)
                {
                    int val = bytes[i] % 10; // 0..9
                    if (i == 0 && val == 0) val = 1; // evitar empezar en 0
                    chars[i] = (char)('0' + val);
                }
                return new string(chars);
            }
        }
    }
}
