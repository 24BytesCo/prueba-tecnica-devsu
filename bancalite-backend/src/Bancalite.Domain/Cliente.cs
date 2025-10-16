namespace Bancalite.Domain;

public class Cliente : BaseEntity
{

    public string? PasswordHash { get; set; }
    public bool Estado { get; set; } = true;

    public Guid RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public Guid PersonaId { get; set; }
    public Persona Persona { get; set; } = null!;

    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
}
