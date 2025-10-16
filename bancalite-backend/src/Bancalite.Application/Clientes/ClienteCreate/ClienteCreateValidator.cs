using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Bancalite.Persitence;

namespace Bancalite.Application.Clientes.ClienteCreate
{
    /// <summary>
    /// Validador de reglas de negocio y formato para crear un cliente.
    /// Incluye validaciones de llaves foráneas contra la base de datos.
    /// </summary>
    public class ClienteCreateValidator : AbstractValidator<ClienteCreateRequest>
    {
        private readonly BancaliteContext _context;

        /// <summary>
        /// Crea una nueva instancia del validador de creación de clientes.
        /// </summary>
        public ClienteCreateValidator(BancaliteContext context)
        {
            _context = context;

            // Reglas básicas de formato/contenido
            RuleFor(x => x.Nombres)
                .NotEmpty().WithMessage("Nombres es requerido")
                .MaximumLength(120);

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Apellidos es requerido")
                .MaximumLength(120);

            RuleFor(x => x.Edad)
                .GreaterThanOrEqualTo(0).WithMessage("Edad no puede ser negativa");

            RuleFor(x => x.NumeroDocumento)
                .NotEmpty().WithMessage("NumeroDocumento es requerido")
                .MaximumLength(50);

            RuleFor(x => x.Direccion)
                .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Direccion));

            RuleFor(x => x.Telefono)
                .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Telefono));

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email es requerido")
                .EmailAddress().WithMessage("Email no es válido");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password es requerido")
                .MinimumLength(6).WithMessage("Password mínimo 6 caracteres");

            // Llave foránea: TipoDocumentoIdentidad (obligatoria)
            RuleFor(x => x.TipoDocumentoIdentidad)
                .NotEmpty().WithMessage("TipoDocumentoIdentidad es requerido")
                .MustAsync(ExistTipoDocumentoAsync)
                .WithMessage("TipoDocumentoIdentidad no existe");

            // Llave foránea: Genero (requerida)
            RuleFor(x => x.GeneroId)
                .NotNull().WithMessage("GeneroId es requerido")
                .Must(id => id.HasValue && id.Value != Guid.Empty).WithMessage("GeneroId es inválido")
                .MustAsync(ExistGeneroAsync)
                .WithMessage("GeneroId no existe");

            // Regla de unicidad: (TipoDocumentoIdentidad, NumeroDocumento)
            RuleFor(x => x)
                .MustAsync(DocumentoNoDuplicadoAsync)
                .WithMessage("La persona con el documento indicado ya existe");
        }

        /// <summary>
        /// Verifica si existe el tipo de documento indicado.
        /// </summary>
        private Task<bool> ExistTipoDocumentoAsync(Guid id, CancellationToken ct)
        {
            // Consulta ligera para validar FK
            return _context.TiposDocumentoIdentidad.AsNoTracking()
                .AnyAsync(t => t.Id == id, ct);
        }

        /// <summary>
        /// Verifica si existe el género indicado.
        /// </summary>
        private Task<bool> ExistGeneroAsync(Guid? id, CancellationToken ct)
        {
            // Si no hay valor o es Guid.Empty, inválido
            if (!id.HasValue || id.Value == Guid.Empty)
                return Task.FromResult(false);

            // Consulta ligera para validar FK
            return _context.Generos.AsNoTracking()
                .AnyAsync(g => g.Id == id.Value, ct);
        }

        /// <summary>
        /// Verifica que no exista ya una persona con el mismo tipo y número de documento.
        /// </summary>
        private async Task<bool> DocumentoNoDuplicadoAsync(ClienteCreateRequest req, CancellationToken ct)
        {
            // Si falta información, dejar que otras reglas fallen primero
            if (string.IsNullOrWhiteSpace(req.NumeroDocumento) || req.TipoDocumentoIdentidad == Guid.Empty)
                return true;

            var numero = req.NumeroDocumento.Trim();
            var existe = await _context.Personas.AsNoTracking()
                .AnyAsync(p => p.TipoDocumentoIdentidadId == req.TipoDocumentoIdentidad &&
                               p.NumeroDocumento == numero, ct);
            // Válido cuando NO existe duplicado
            return !existe;
        }
    }
}
