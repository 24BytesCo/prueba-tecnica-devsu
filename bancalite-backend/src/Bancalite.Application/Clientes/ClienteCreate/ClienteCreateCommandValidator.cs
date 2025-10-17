using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Bancalite.Persitence;

namespace Bancalite.Application.Clientes.ClienteCreate
{
    /// <summary>
    /// Validador del mensaje MediatR para creación de cliente.
    /// Contiene todas las reglas (formato + FKs + unicidad) sobre el payload.
    /// </summary>
    public class ClienteCreateCommandValidator : AbstractValidator<ClienteCreateCommand.ClienteCreateCommandRequest>
    {
        private readonly BancaliteContext _context;

        /// <summary>
        /// Crea un validador para el comando de creación de cliente.
        /// </summary>
        public ClienteCreateCommandValidator(BancaliteContext context)
        {
            _context = context;

            // Nombres y Apellidos requeridos
            RuleFor(x => x.clienteCreateRequest.Nombres)
                .NotEmpty().WithMessage("Nombres es requerido")
                .MaximumLength(120);

            RuleFor(x => x.clienteCreateRequest.Apellidos)
                .NotEmpty().WithMessage("Apellidos es requerido")
                .MaximumLength(120);

            // Edad >= 0
            RuleFor(x => x.clienteCreateRequest.Edad)
                .GreaterThanOrEqualTo(0).WithMessage("Edad no puede ser negativa");

            // Documento requerido y longitud
            RuleFor(x => x.clienteCreateRequest.NumeroDocumento)
                .NotEmpty().WithMessage("NumeroDocumento es requerido")
                .MaximumLength(50);

            // Direccion y Telefono opcionales con longitud
            RuleFor(x => x.clienteCreateRequest.Direccion)
                .MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.clienteCreateRequest.Direccion));

            RuleFor(x => x.clienteCreateRequest.Telefono)
                .MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.clienteCreateRequest.Telefono));

            // Email obligatorio (se usa para iniciar sesión) y con formato válido
            RuleFor(x => x.clienteCreateRequest.Email)
                .NotEmpty().WithMessage("Email es requerido")
                .Must(email => !string.IsNullOrWhiteSpace(email)).WithMessage("Email es requerido")
                .EmailAddress().WithMessage("Email no es válido");

            // Unicidad de Email (si se envía): no debe existir en Personas ni debe estar vinculado
            // a un Cliente a través de un AppUser con ese mismo email
            RuleFor(x => x.clienteCreateRequest.Email)
                .MustAsync(async (email, ct) =>
                {
                    if (string.IsNullOrWhiteSpace(email)) return false; // ya lo cubre NotEmpty
                    var mail = email.Trim();

                    // 1) Email en Personas
                    var personaDup = await _context.Personas.AsNoTracking()
                        .AnyAsync(p => p.Email != null && p.Email == mail, ct);
                    if (personaDup) return false;

                    // 2) Email en Users y ya vinculado a un Cliente
                    var userId = await _context.Users.AsNoTracking()
                        .Where(u => u.Email == mail)
                        .Select(u => u.Id)
                        .FirstOrDefaultAsync(ct);
                    if (userId != Guid.Empty)
                    {
                        var vinculada = await _context.Clientes.AsNoTracking()
                            .AnyAsync(c => c.AppUserId == userId, ct);
                        if (vinculada) return false;
                    }

                    return true;
                })
                .WithMessage("El email ya está registrado para otro cliente");

            // Password opcional: si viene, longitud mínima
            RuleFor(x => x.clienteCreateRequest.Password)
                .MinimumLength(6).WithMessage("Password mínimo 6 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.clienteCreateRequest.Password));

            // FK: TipoDocumentoIdentidad requerido y debe existir
            RuleFor(x => x.clienteCreateRequest.TipoDocumentoIdentidad)
                .NotEmpty().WithMessage("TipoDocumentoIdentidad es requerido")
                .MustAsync(async (id, ct) => await _context.TiposDocumentoIdentidad.AsNoTracking().AnyAsync(t => t.Id == id, ct))
                .WithMessage("TipoDocumentoIdentidad no existe");

            // FK: Genero requerido y debe existir
            RuleFor(x => x.clienteCreateRequest.GeneroId)
                .NotNull().WithMessage("GeneroId es requerido")
                .Must(id => id.HasValue && id.Value != Guid.Empty).WithMessage("GeneroId es inválido")
                .MustAsync(async (id, ct) => id.HasValue && await _context.Generos.AsNoTracking().AnyAsync(g => g.Id == id.Value, ct))
                .WithMessage("GeneroId no existe");

            // Unicidad de (TipoDocumentoIdentidad, NumeroDocumento)
            RuleFor(x => x.clienteCreateRequest)
                .MustAsync(async (req, ct) =>
                {
                    if (string.IsNullOrWhiteSpace(req.NumeroDocumento) || req.TipoDocumentoIdentidad == Guid.Empty)
                        return true; // Dejar que otras reglas fallen primero

                    var numero = req.NumeroDocumento.Trim();
                    var existe = await _context.Personas.AsNoTracking()
                        .AnyAsync(p => p.TipoDocumentoIdentidadId == req.TipoDocumentoIdentidad && p.NumeroDocumento == numero, ct);
                    return !existe;
                })
                .WithMessage("La persona con el documento indicado ya existe");
        }
    }
}
