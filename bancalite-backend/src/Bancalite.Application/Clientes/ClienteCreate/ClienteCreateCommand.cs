using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bancalite.Persitence;
using Bancalite.Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Bancalite.Persitence.Model;

namespace Bancalite.Application.Clientes.ClienteCreate
{
    /// <summary>
    /// Comando para crear un nuevo cliente.
    /// </summary>
    public class ClienteCreateCommand
    {
        /// <summary>
        /// Mensaje de solicitud para crear un cliente (CQRS/MediatR).
        /// </summary>
        /// <param name="clienteCreateRequest">Datos de entrada para crear el cliente.</param>
        public record ClienteCreateCommandRequest(ClienteCreateRequest clienteCreateRequest) : IRequest<Bancalite.Application.Core.Result<Guid>>;

        /// <summary>
        /// Manejador del comando de creación de cliente.
        /// </summary>
        internal class ClienteCreateCommandHandler : IRequestHandler<ClienteCreateCommandRequest, Bancalite.Application.Core.Result<Guid>>
        {
            private readonly BancaliteContext _context;
            private readonly UserManager<AppUser> _userManager;
            private readonly RoleManager<IdentityRole<Guid>> _roleManager;

            public ClienteCreateCommandHandler(BancaliteContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
            {
                _context = context;
                _userManager = userManager;
                _roleManager = roleManager;
            }

            /// <summary>
            /// Ejecuta la creación de un cliente.
            /// </summary>
            /// <remarks>
            /// - Valida entrada vía pipeline (FluentValidation).
            /// - Crea entidad Persona y luego Cliente.
            /// - Si se envía Email/Password, crea o vincula un AppUser en Identity.
            /// - Asigna rol (por defecto 'User' o según IdRol).
            /// - Copia <c>PasswordHash</c> del AppUser hacia Cliente según especificación.
            /// - Persiste de forma transaccional y retorna el Id del Cliente.
            /// </remarks>
            public async Task<Bancalite.Application.Core.Result<Guid>> Handle(ClienteCreateCommandRequest request, CancellationToken cancellationToken)
            {
                // Validaciones se ejecutan automáticamente vía pipeline de MediatR (ValidationBehavior)

                try
                {
                    // Se mapea la información a entidades de dominio
                    var persona = new Persona
                    {
                        Id = Guid.NewGuid(),
                        Nombres = request.clienteCreateRequest.Nombres!.Trim(), 
                        Apellidos = request.clienteCreateRequest.Apellidos!.Trim(), 
                        Edad = request.clienteCreateRequest.Edad,
                        GeneroId = request.clienteCreateRequest.GeneroId!.Value,
                        TipoDocumentoIdentidadId = request.clienteCreateRequest.TipoDocumentoIdentidad,
                        NumeroDocumento = request.clienteCreateRequest.NumeroDocumento!.Trim(),
                        Direccion = string.IsNullOrWhiteSpace(request.clienteCreateRequest.Direccion)
                            ? null : request.clienteCreateRequest.Direccion!.Trim(),
                        Telefono = string.IsNullOrWhiteSpace(request.clienteCreateRequest.Telefono)
                            ? null : request.clienteCreateRequest.Telefono!.Trim(),
                        Email = string.IsNullOrWhiteSpace(request.clienteCreateRequest.Email)
                            ? null : request.clienteCreateRequest.Email!.Trim()
                    };

                    // Se crea o vincula el usuario de Identity
                    AppUser? appUser = null;
                    if (!string.IsNullOrWhiteSpace(request.clienteCreateRequest.Email))
                    {
                        var email = request.clienteCreateRequest.Email!.Trim();
                        appUser = await _userManager.FindByEmailAsync(email);
                        if (appUser == null)
                        {
                            var userName = request.clienteCreateRequest.NumeroDocumento ?? email;
                            appUser = new AppUser
                            {
                                Id = Guid.NewGuid(),
                                Email = email,
                                UserName = userName,
                                DisplayName = $"{persona.Nombres} {persona.Apellidos}".Trim()
                            };
                            var hasPassword = !string.IsNullOrWhiteSpace(request.clienteCreateRequest.Password);
                            var createRes = hasPassword
                                ? await _userManager.CreateAsync(appUser, request.clienteCreateRequest.Password!)
                                : await _userManager.CreateAsync(appUser);
                            if (!createRes.Succeeded)
                            {
                                var err = string.Join("; ", createRes.Errors.Select(e => e.Description));
                                throw new InvalidOperationException($"No se pudo crear el usuario de Identity: {err}");
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(request.clienteCreateRequest.Password))
                        {
                            // Si ya existe, agregar password solo si no tiene
                            if (!await _userManager.HasPasswordAsync(appUser))
                            {
                                var addPass = await _userManager.AddPasswordAsync(appUser, request.clienteCreateRequest.Password!);
                                if (!addPass.Succeeded)
                                {
                                    var err = string.Join("; ", addPass.Errors.Select(e => e.Description));
                                    throw new InvalidOperationException($"No se pudo asignar password al usuario existente: {err}");
                                }
                            }
                        }

                        // Se determina el rol: IdRol o 'User' por defecto
                        string roleName = "User";
                        if (request.clienteCreateRequest.IdRol.HasValue)
                        {
                            var role = await _roleManager.FindByIdAsync(request.clienteCreateRequest.IdRol.Value.ToString());
                            if (role != null) roleName = role.Name!;
                        }

                        // Se asegura que el rol exista
                        var roleExists = await _roleManager.RoleExistsAsync(roleName);
                        if (!roleExists)
                        {
                            var created = await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                            if (!created.Succeeded)
                            {
                                var err = string.Join("; ", created.Errors.Select(e => e.Description));
                                throw new InvalidOperationException($"No se pudo crear el rol '{roleName}': {err}");
                            }
                        }

                        // Se asigna el rol si no lo tiene
                        if (!await _userManager.IsInRoleAsync(appUser, roleName))
                        {
                            var addRole = await _userManager.AddToRoleAsync(appUser, roleName);
                            if (!addRole.Succeeded)
                            {
                                var err = string.Join("; ", addRole.Errors.Select(e => e.Description));
                                throw new InvalidOperationException($"No se pudo asignar el rol '{roleName}' al usuario: {err}");
                            }
                        }
                    }

                    var cliente = new Cliente
                    {
                        Id = Guid.NewGuid(),
                        Persona = persona,
                        PersonaId = persona.Id,
                        AppUserId = appUser?.Id,

                        // Se guarda el hash como pide especificación
                        PasswordHash = appUser?.PasswordHash,
                        Estado = true
                    };

                    // Se guarda en la base de datos (transaccional por SaveChanges)
                    await _context.Personas.AddAsync(persona, cancellationToken);
                    await _context.Clientes.AddAsync(cliente, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    // Retornar el Id del cliente creado
                    return Bancalite.Application.Core.Result<Guid>.Success(cliente.Id);
                }
                catch (DbUpdateException ex)
                {
                    // Error de persistencia (FK/UK, etc.)
                    return Bancalite.Application.Core.Result<Guid>.Failure("No se pudo crear el cliente en la base de datos");
                }
            }
        }
    }
}

