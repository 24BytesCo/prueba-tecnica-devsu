using Bancalite.Persitence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Application.Clientes.ClienteUpdate
{
    /// <summary>
    /// Validaciones para actualización total de cliente (PUT).
    /// </summary>
    public class ClientePutValidator : AbstractValidator<ClientePutRequest>
    {
        private readonly BancaliteContext _context;

        /// <summary>
        /// Crea una nueva instancia del validador para PUT de cliente.
        /// </summary>
        /// <param name="context">Contexto de datos para validar FKs y reglas.</param>
        public ClientePutValidator(BancaliteContext context)
        {
            _context = context;

            RuleFor(x => x.Nombres).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Apellidos).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Edad).GreaterThanOrEqualTo(0);
            RuleFor(x => x.GeneroId)
                .NotEmpty().MustAsync(async (id, ct) => await _context.Generos.AsNoTracking().AnyAsync(g => g.Id == id, ct))
                .WithMessage("GeneroId no existe");
            RuleFor(x => x.TipoDocumentoIdentidadId)
                .NotEmpty().MustAsync(async (id, ct) => await _context.TiposDocumentoIdentidad.AsNoTracking().AnyAsync(t => t.Id == id, ct))
                .WithMessage("TipoDocumentoIdentidadId no existe");
            RuleFor(x => x.NumeroDocumento).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Direccion).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Direccion));
            RuleFor(x => x.Telefono).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Telefono));
            RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        }
    }

    /// <summary>
    /// Validaciones para actualización parcial (PATCH) — valida formato cuando se envía.
    /// </summary>
    public class ClientePatchValidator : AbstractValidator<ClientePatchRequest>
    {
        private readonly BancaliteContext _context;

        /// <summary>
        /// Crea una nueva instancia del validador para PATCH de cliente.
        /// </summary>
        /// <param name="context">Contexto de datos para validar FKs y reglas.</param>
        public ClientePatchValidator(BancaliteContext context)
        {
            _context = context;

            RuleFor(x => x.Nombres).MaximumLength(120).When(x => x.Nombres != null);
            RuleFor(x => x.Apellidos).MaximumLength(120).When(x => x.Apellidos != null);
            RuleFor(x => x.Edad).GreaterThanOrEqualTo(0).When(x => x.Edad.HasValue);
            RuleFor(x => x.GeneroId)
                .MustAsync(async (id, ct) => id.HasValue && await _context.Generos.AsNoTracking().AnyAsync(g => g.Id == id.Value, ct))
                .When(x => x.GeneroId.HasValue)
                .WithMessage("GeneroId no existe");
            RuleFor(x => x.TipoDocumentoIdentidadId)
                .MustAsync(async (id, ct) => id.HasValue && await _context.TiposDocumentoIdentidad.AsNoTracking().AnyAsync(t => t.Id == id.Value, ct))
                .When(x => x.TipoDocumentoIdentidadId.HasValue)
                .WithMessage("TipoDocumentoIdentidadId no existe");
            RuleFor(x => x.NumeroDocumento).MaximumLength(50).When(x => x.NumeroDocumento != null);
            RuleFor(x => x.Direccion).MaximumLength(200).When(x => x.Direccion != null && !string.IsNullOrWhiteSpace(x.Direccion));
            RuleFor(x => x.Telefono).MaximumLength(50).When(x => x.Telefono != null && !string.IsNullOrWhiteSpace(x.Telefono));
            RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null && !string.IsNullOrWhiteSpace(x.Email));
        }
    }
}
