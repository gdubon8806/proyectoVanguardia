using FastFix;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registrar el contexto de la base de datos
builder.Services.AddDbContext<FastFixDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")
    ));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider
            .GetRequiredService<FastFixDbContext>();

        db.Database.Migrate();
    }
}

// Swagger

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();