using Microsoft.EntityFrameworkCore;
using FastFix.Models;

namespace FastFix;

public class FastFixDbContext : DbContext
{
    public FastFixDbContext(DbContextOptions<FastFixDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();

    public DbSet<SolicitudServicio> Solicitudes => Set<SolicitudServicio>();
}