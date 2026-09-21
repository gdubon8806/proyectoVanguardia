using FastFix.Models;

namespace FastFix.Models;

public class SolicitudServicio
{
    public int Id { get; set; }

    public string DescripcionProblema { get; set; } = string.Empty;

    public string Estado { get; set; } = "Pendiente";

    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public int? TecnicoId { get; set; }

    public Tecnico? Tecnico { get; set; }
}