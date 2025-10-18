using Bancalite.Application.Core;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace Bancalite.WebApi.Middleware
{

    // Middleware para manejo global de excepciones no controladas
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;


        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FluentValidation.ValidationException vex)
            {
                // Errores de validación (400) agregados en una sola cadena
                _logger.LogWarning(vex, "Validación fallida: {Mensaje}", vex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                // Tomar mensajes distintos y unir por comas
                var lista = vex.Errors
                    .Select(e => e.ErrorMessage)
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Distinct()
                    .ToList();
                var mensaje = lista.Count > 0 ? string.Join(", ", lista) : "Solicitud inválida";

                // Mantener misma estructura del resto del manejador (AppException)
                var payload = new AppException(context.Response.StatusCode, mensaje);
                var opciones = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(payload, opciones);
                await context.Response.WriteAsync(json);
                return;
            }
            catch (Exception ex)
            {
                // Loguear la excepción
                _logger.LogError(ex, "Se ha producido una excepción no controlada: {Mensaje}", ex.Message);

                // Respuesta JSON con código de estado y mensaje
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // En desarrollo se muestra mensaje y stacktrace, en producción solo mensaje genérico
                var respuesta = _environment.IsDevelopment()
                    ? new AppException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new AppException(context.Response.StatusCode, "Error Interno del Servidor");

                // Opciones de serialización JSON (camelCase)
                var opciones = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                // Serializando la respuesta
                var json = JsonSerializer.Serialize(respuesta, opciones);

                // Se escribe la respuesta
                await context.Response.WriteAsync(json);
            }
        }

    }
}

