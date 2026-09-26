using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastFix.Models;
using FastFix.Validators;

namespace FastFix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TecnicosController : ControllerBase
{
    private readonly FastFixDbContext _db;

    public TecnicosController(FastFixDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tecnicos = await _db.Tecnicos.ToListAsync();
        return Ok(tecnicos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El identificador debe ser mayor que cero.");

        var tecnico = await _db.Tecnicos.FindAsync(id);

        if (tecnico is null)
            return NotFound();

        return Ok(tecnico);
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles()
    {
        var tecnicos = await _db.Tecnicos
            .Where(t =>
                t.Disponible &&
                _db.Solicitudes.Count(s =>
                    s.TecnicoId == t.Id &&
                    (s.Estado == "Asignada" ||
                     s.Estado == "En Proceso")) < 3)
            .ToListAsync();

        return Ok(tecnicos);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Tecnico tecnico)
    {
        var error = TecnicoValidator.Validar(tecnico);

        if (error is not null)
            return BadRequest(error);

        tecnico.Nombre = tecnico.Nombre.Trim();

        _db.Tecnicos.Add(tecnico);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = tecnico.Id },
            tecnico);
    }

    [HttpPut("{id}/disponibilidad")]
    public async Task<IActionResult> CambiarDisponibilidad(
        int id,
        [FromQuery] bool disponible)
    {
        var tecnico = await _db.Tecnicos.FindAsync(id);

        if (tecnico is null)
            return NotFound();

        tecnico.Disponible = disponible;

        await _db.SaveChangesAsync();

        return Ok(tecnico);
    }
}