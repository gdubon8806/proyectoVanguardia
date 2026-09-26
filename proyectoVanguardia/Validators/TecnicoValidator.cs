using FastFix.Models;

namespace FastFix.Validators;

public static class TecnicoValidator
{
    public static string? Validar(Tecnico tecnico)
    {
        if (string.IsNullOrWhiteSpace(tecnico.Nombre))
            return "El nombre del técnico es obligatorio.";

        return null;
    }
}