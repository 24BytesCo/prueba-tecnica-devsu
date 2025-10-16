namespace Bancalite.Domain;

public class Persona : BaseEntity
{
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public int Edad { get; set; }

    public Guid GeneroId { get; set; }
    public Genero Genero { get; set; } = null!;

    public Guid TipoDocumentoIdentidadId { get; set; }
    public TipoDocumentoIdentidad TipoDocumentoIdentidad { get; set; } = null!;

    public string NumeroDocumento { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    // Navegación 1-1 con Cliente
    public Cliente? Cliente { get; set; }
}

