using System.Reflection;
using Bancalite.Infraestructure;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI (Swashbuckle) + comentarios XML
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Bancalite API",
        Version = "v1",
        Description = "API para Banca (Clientes, Cuentas, Movimientos, Reportes)"
    });

    // Comentarios XML (<summary/> y <remarks/>) — se muestran en Swagger
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    // Seguridad: esquema Bearer JWT (si se habilita Auth en la API)
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT como: Bearer {token}"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new List<string>()
        }
    });
});

// Health Checks (liveness/readiness básicos)
builder.Services.AddHealthChecks();

// Infraestructura (DbContext, migraciones/seed en Development vía HostedService)
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

// Swagger UI en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bancalite API v1");
        options.RoutePrefix = "swagger"; // launchSettings apunta a /swagger
    });
    // Seeding se ejecuta en Infrastructure (HostedService)
}

// Endpoint de salud simple
app.MapHealthChecks("/health");

// Alias /status con respuesta JSON mínima
app.MapGet("/status", () => Results.Json(new
{
    status = "ok",
    service = "Bancalite.WebApi",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString()
}));

app.Run();
