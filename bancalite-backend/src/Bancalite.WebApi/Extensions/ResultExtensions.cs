using Bancalite.Application.Core;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Bancalite.WebApi.Extensions
{
    /// <summary>
    /// Extensiones para convertir Result genérico del application layer a respuestas HTTP coherentes.
    /// Mantiene los controladores delgados (sin lógica de branching).
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Mapea un Result genérico a ActionResult envolviendo el Result en el cuerpo.
        /// OK => 200 con Result, Errores => 4xx adecuados con Result.
        /// </summary>
        public static ActionResult<Result<T>> FromResult<T>(this ControllerBase controller, Result<T> result)
        {
            if (result == null) return controller.BadRequest(Result<T>.Failure("Solicitud inválida"));

            if (result.IsSuccess)
                return controller.Ok(result);

            var error = result.Error ?? string.Empty;

            var (status, title) = MapStatus(error);
            var pd = new ProblemDetails
            {
                Title = title,
                Detail = error,
                Status = status
            };
            return new ObjectResult(pd) { StatusCode = status };
        }

        /// <summary>
        /// Mapea un Result genérico a ActionResult retornando solo Datos en éxito.
        /// Útil para endpoints que deben devolver el DTO plano (p.ej., login/refresh).
        /// </summary>
        public static ActionResult FromResultData<T>(this ControllerBase controller, Result<T> result)
        {
            if (result == null) return controller.BadRequest();

            if (result.IsSuccess)
                return controller.Ok(result.Datos);

            var error = result.Error ?? string.Empty;

            var (status, title) = MapStatus(error);
            var pd = new ProblemDetails
            {
                Title = title,
                Detail = error,
                Status = status
            };
            return new ObjectResult(pd) { StatusCode = status };
        }

        private static (int Status, string Title) MapStatus(string error)
        {
            if (error.StartsWith("Unauthorized", StringComparison.OrdinalIgnoreCase)) return (401, "Unauthorized");
            if (error.StartsWith("Forbidden", StringComparison.OrdinalIgnoreCase)) return (403, "Forbidden");
            if (error.StartsWith("Unprocessable", StringComparison.OrdinalIgnoreCase)) return (422, "Unprocessable Entity");
            if (error.StartsWith("No encontrado", StringComparison.OrdinalIgnoreCase) || error.IndexOf("no encontrado", StringComparison.OrdinalIgnoreCase) >= 0) return (404, "Not Found");
            if (error.StartsWith("Conflict", StringComparison.OrdinalIgnoreCase) || error.IndexOf("duplicidad", StringComparison.OrdinalIgnoreCase) >= 0 || error.IndexOf("ya existe", StringComparison.OrdinalIgnoreCase) >= 0) return (409, "Conflict");
            return (400, "Bad Request");
        }
    }
}
