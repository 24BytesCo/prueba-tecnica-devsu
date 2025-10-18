namespace Bancalite.Domain;

public class Rol : BaseEntity
{
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
