using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Bancalite.Application.Core.Behaviors;
using AutoMapper;

namespace Bancalite.Application
{
    public static class DependencyInjection
    {
        /// <summary>
        /// Registra servicios de la capa Application (MediatR, Validadores, etc.).
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // MediatR (registrar handlers del ensamblado Application)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            // FluentValidation: registra todos los validadores del ensamblado
            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

            // Pipeline: Validación automática para cada request de MediatR
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // AutoMapper: perfiles del ensamblado Application
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
