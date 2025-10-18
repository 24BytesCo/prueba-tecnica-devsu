namespace Bancalite.Domain;

public class TipoDocumentoIdentidad : BaseEntity
{
    public string Codigo { get; set; } = null!; // UK
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Persona> Personas { get; set; } = new List<Persona>();
}
