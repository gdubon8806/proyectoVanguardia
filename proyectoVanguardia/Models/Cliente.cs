using FastFix.Models;
using System.Text.Json.Serialization;

namespace FastFix.Models;

public class Cliente
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    [JsonIgnore]
    public List<SolicitudServicio> Solicitudes { get; set; } = new();
}