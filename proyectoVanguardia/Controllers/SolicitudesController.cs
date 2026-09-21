using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastFix.Models;

namespace FastFix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolicitudesController : ControllerBase
{
    private const string Pendiente = "Pendiente";
    private const string Asignada = "Asignada";
    private const string EnProceso = "En Proceso";
    private const string Completada = "Completada";

    private readonly FastFixDbContext _db;

    public SolicitudesController(FastFixDbContext db)
    {
        _db = db;
    }

    // Listar solicitudes y filtrar por estado
    [HttpGet]
    public async Task<IActionResult> GetAll(string? estado)
    {
        var query = _db.Solicitudes
            .Include(s => s.Cliente)
            .Include(s => s.Tecnico)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = NormalizarEstado(estado);

            if (estadoNormalizado is null)
                return BadRequest("El estado indicado no es válido.");

            query = query.Where(s => s.Estado == estadoNormalizado);
        }

        var solicitudes = await query.ToListAsync();

        return Ok(solicitudes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El identificador debe ser mayor que cero.");

        var solicitud = await _db.Solicitudes
            .Include(s => s.Cliente)
            .Include(s => s.Tecnico)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitud is null)
            return NotFound();

        return Ok(solicitud);
    }

    // Crear solicitud
    [HttpPost]
    public async Task<IActionResult> Create(SolicitudServicio solicitud)
    {
        AplicarTransformacion(solicitud);

        var error = ValidarSolicitud(solicitud);

        if (error is not null)
            return BadRequest(error);

        var cliente = await _db.Clientes.FindAsync(solicitud.ClienteId);

        if (cliente is null)
            return BadRequest("El cliente indicado no existe.");

        var errorCliente = ValidarCliente(cliente);

        if (errorCliente is not null)
            return BadRequest(errorCliente);

        // Toda solicitud nueva comienza pendiente.
        solicitud.Estado = Pendiente;
        solicitud.TecnicoId = null;

        _db.Solicitudes.Add(solicitud);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = solicitud.Id },
            solicitud);
    }

    // Modificar datos básicos de la solicitud
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        SolicitudServicio solicitudActualizada)
    {
        if (id <= 0)
            return BadRequest("El identificador debe ser mayor que cero.");

        var solicitud = await _db.Solicitudes.FindAsync(id);

        if (solicitud is null)
            return NotFound();

        // Regla: una solicitud completada no se puede modificar.
        if (solicitud.Estado == Completada)
            return Conflict(
                "Una solicitud completada no puede modificarse.");

        AplicarTransformacion(solicitudActualizada);

        var error = ValidarSolicitud(solicitudActualizada);

        if (error is not null)
            return BadRequest(error);

        var cliente = await _db.Clientes
            .FindAsync(solicitudActualizada.ClienteId);

        if (cliente is null)
            return BadRequest("El cliente indicado no existe.");

        solicitud.ClienteId = solicitudActualizada.ClienteId;
        solicitud.DescripcionProblema =
            solicitudActualizada.DescripcionProblema;

        await _db.SaveChangesAsync();

        return Ok(solicitud);
    }

    // Asignar técnico
    [HttpPut("{id}/asignar/{tecnicoId}")]
    public async Task<IActionResult> AsignarTecnico(
        int id,
        int tecnicoId)
    {
        var solicitud = await _db.Solicitudes.FindAsync(id);

        if (solicitud is null)
            return NotFound("La solicitud no existe.");

        // Una solicitud completada no puede modificarse.
        if (solicitud.Estado == Completada)
            return Conflict(
                "Una solicitud completada no puede modificarse.");

        var tecnico = await _db.Tecnicos.FindAsync(tecnicoId);

        if (tecnico is null)
            return NotFound("El técnico no existe.");

        // Regla: solo técnico marcado como disponible.
        if (!tecnico.Disponible)
            return Conflict(
                "El técnico no está disponible.");

        // Regla: máximo 3 solicitudes activas.
        var solicitudesActivas = await _db.Solicitudes.CountAsync(s =>
            s.TecnicoId == tecnicoId &&
            s.Id != id &&
            (s.Estado == Asignada ||
             s.Estado == EnProceso));

        if (solicitudesActivas >= 3)
            return Conflict(
                "El técnico ya tiene 3 solicitudes activas.");

        solicitud.TecnicoId = tecnicoId;
        solicitud.Estado = Asignada;

        await _db.SaveChangesAsync();

        return Ok(solicitud);
    }

    // Marcar solicitud como completada
    [HttpPut("{id}/completar")]
    public async Task<IActionResult> Completar(int id)
    {
        var solicitud = await _db.Solicitudes.FindAsync(id);

        if (solicitud is null)
            return NotFound();

        if (solicitud.Estado == Completada)
            return Conflict(
                "La solicitud ya está completada.");

        // Regla: no puede completarse sin técnico.
        if (solicitud.TecnicoId is null)
            return Conflict(
                "La solicitud no puede completarse sin un técnico asignado.");

        solicitud.Estado = Completada;

        await _db.SaveChangesAsync();

        return Ok(solicitud);
    }

    // Eliminar solicitud
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest(
                "El identificador debe ser mayor que cero.");

        var solicitud = await _db.Solicitudes.FindAsync(id);

        if (solicitud is null)
            return NotFound();

        // Regla: las completadas no se pueden eliminar.
        if (solicitud.Estado == Completada)
            return Conflict(
                "Una solicitud completada no puede eliminarse.");

        _db.Solicitudes.Remove(solicitud);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // ----------------------------
    // Transformaciones
    // ----------------------------

    private static void AplicarTransformacion(
        SolicitudServicio solicitud)
    {
        solicitud.DescripcionProblema = Regex.Replace(
            solicitud.DescripcionProblema?.Trim() ??
            string.Empty,
            @"\s+",
            " ");
    }

    // ----------------------------
    // Validaciones
    // ----------------------------

    private static string? ValidarSolicitud(
        SolicitudServicio solicitud)
    {
        if (solicitud.ClienteId <= 0)
            return "Debe indicar un cliente válido.";

        if (string.IsNullOrWhiteSpace(
            solicitud.DescripcionProblema))
            return "La descripción del problema es obligatoria.";

        if (solicitud.DescripcionProblema.Length < 10)
            return "La descripción debe tener al menos 10 caracteres.";

        return null;
    }

    private static string? ValidarCliente(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nombre))
            return "El nombre del cliente es obligatorio.";

        if (string.IsNullOrWhiteSpace(cliente.Telefono))
            return "El teléfono del cliente es obligatorio.";

        if (!Regex.IsMatch(cliente.Telefono, @"^\d{8}$"))
            return "El teléfono debe tener 8 dígitos.";

        return null;
    }

    // ----------------------------
    // Estados
    // ----------------------------

    private static string? NormalizarEstado(string estado)
    {
        if (estado.Equals(
            Pendiente,
            StringComparison.OrdinalIgnoreCase))
            return Pendiente;

        if (estado.Equals(
            Asignada,
            StringComparison.OrdinalIgnoreCase))
            return Asignada;

        if (estado.Equals(
            EnProceso,
            StringComparison.OrdinalIgnoreCase))
            return EnProceso;

        if (estado.Equals(
            Completada,
            StringComparison.OrdinalIgnoreCase))
            return Completada;

        return null;
    }
}