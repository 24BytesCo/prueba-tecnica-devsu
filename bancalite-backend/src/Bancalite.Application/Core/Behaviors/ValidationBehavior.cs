using FluentValidation;
using MediatR;

namespace Bancalite.Application.Core.Behaviors
{
    /// <summary>
    /// Pipeline de MediatR para validar automáticamente cada request
    /// usando los validadores de FluentValidation registrados en DI.
    /// </summary>
    /// <typeparam name="TRequest">Tipo del request.</typeparam>
    /// <typeparam name="TResponse">Tipo de la respuesta.</typeparam>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        /// <summary>
        /// Crea una instancia del pipeline de validación.
        /// </summary>
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        /// <summary>
        /// Ejecuta las validaciones antes del handler. Si hay errores, lanza ValidationException.
        /// </summary>
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Si hay validadores registrados para este TRequest
            if (_validators.Any())
            {
                // Validar en paralelo
                var context = new ValidationContext<TRequest>(request);
                var tasks = _validators.Select(v => v.ValidateAsync(context, cancellationToken));
                var results = await Task.WhenAll(tasks);
                var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

                if (failures.Count > 0)
                {
                    // Lanzar excepción con el detalle de errores
                    throw new ValidationException(failures);
                }
            }

            // Continuar con el siguiente comportamiento/handler
            return await next();
        }
    }
}

