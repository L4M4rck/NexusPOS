using Microsoft.OpenApi;
using NexusPOS.Api.Middleware;
using NexusPOS.Infrastructure;
using NexusPOS.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Registra las capacidades de la API y delega a Infrastructure la configuración
// de MySQL, JWT y las implementaciones de los casos de uso.
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    });
});

// Swagger describe los endpoints y permite probar rutas protegidas enviando un JWT Bearer.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "NexusPOS API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Ingrese únicamente el JWT; Swagger agrega el prefijo Bearer."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// El orden del pipeline es importante: los errores envuelven toda la petición y
// autenticación debe ejecutarse antes de comprobar permisos.
app.UseMiddleware<ProblemDetailsMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

if (!app.Environment.IsEnvironment("Testing"))
{
    // En ejecución normal aplica migraciones pendientes y crea los datos iniciales.
    // Testing administra su propia base para mantener las pruebas aisladas.
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DbInitializer>().InitializeAsync();
}

await app.RunAsync();

public partial class Program;
