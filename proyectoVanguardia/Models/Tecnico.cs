using FastFix.Models;
using System.Text.Json.Serialization;

namespace FastFix.Models;

public class Tecnico
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Disponible { get; set; } = true;

    [JsonIgnore]
    public List<SolicitudServicio> Solicitudes { get; set; } = new();
}