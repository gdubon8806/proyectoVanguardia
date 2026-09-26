using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FastFix.Models;
using FastFix.Validators;

namespace FastFix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly FastFixDbContext _db;

    public ClientesController(FastFixDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clientes = await _db.Clientes.ToListAsync();
        return Ok(clientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("El identificador debe ser mayor que cero.");

        var cliente = await _db.Clientes.FindAsync(id);

        if (cliente is null)
            return NotFound();

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        AplicarTransformacion(cliente);

        var error = ClienteValidator.Validar(cliente);

        if (error is not null)
            return BadRequest(error);

        _db.Clientes.Add(cliente);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = cliente.Id },
            cliente);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Cliente clienteActualizado)
    {
        if (id <= 0)
            return BadRequest("El identificador debe ser mayor que cero.");

        var cliente = await _db.Clientes.FindAsync(id);

        if (cliente is null)
            return NotFound();

        AplicarTransformacion(clienteActualizado);

        var error = ClienteValidator.Validar(clienteActualizado);

        if (error is not null)
            return BadRequest(error);

        cliente.Nombre = clienteActualizado.Nombre;
        cliente.Telefono = clienteActualizado.Telefono;

        await _db.SaveChangesAsync();

        return Ok(cliente);
    }

    private static void AplicarTransformacion(Cliente cliente)
    {
        cliente.Nombre = Regex.Replace(
            cliente.Nombre?.Trim() ?? string.Empty,
            @"\s+",
            " ");

        cliente.Telefono = cliente.Telefono?.Trim() ?? string.Empty;
    }
}