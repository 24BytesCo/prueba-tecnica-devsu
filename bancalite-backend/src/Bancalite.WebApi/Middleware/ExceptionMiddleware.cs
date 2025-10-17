using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Bancalite.Application.Core;
using System.Security.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

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

