using System.Text.RegularExpressions;
using FastFix.Models;

namespace FastFix.Validators;

public static class ClienteValidator
{
    public static string? Validar(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            return "El nombre del cliente es obligatorio.";

        if (string.IsNullOrWhiteSpace(cliente.Telefono))
            return "El teléfono del cliente es obligatorio.";

        if (!Regex.IsMatch(cliente.Telefono, @"^\d{8}$"))
            return "El teléfono debe tener 8 dígitos.";

        return null;
    }
}