namespace Bancalite.Domain;

public class Cliente : BaseEntity
{
    public string? PasswordHash { get; set; }
    public bool Estado { get; set; } = true;

    // Vínculo opcional con el usuario de Identity
    public Guid? AppUserId { get; set; }

    public Guid PersonaId { get; set; }
    public Persona Persona { get; set; } = null!;

    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
}
