using FastFix.Models;

namespace FastFix.Validators;

public static class SolicitudValidator
{
    public static string? Validar(SolicitudServicio solicitud)
    {
        if (solicitud.ClienteId <= 0)
            return "Debe indicar un cliente válido.";

        if (string.IsNullOrWhiteSpace(solicitud.DescripcionProblema))
            return "La descripción del problema es obligatoria.";

        if (solicitud.DescripcionProblema.Length < 10)
            return "La descripción debe tener al menos 10 caracteres.";

        return null;
    }
}