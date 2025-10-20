# =================================================================
# Dockerfile para la API de Bancalite
# =================================================================

# --- Etapa 1: Build Stage ---
# Se utiliza la imagen del .NET SDK que contiene todas las herramientas
# necesarias para restaurar dependencias, compilar y publicar el proyecto.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia los archivos de proyecto (.csproj) individualmente.
# Esto optimiza el sistema de caché por capas de Docker. El paso 'dotnet restore'
COPY ./bancalite-backend/src/Bancalite.Domain/Bancalite.Domain.csproj ./src/Bancalite.Domain/
COPY ./bancalite-backend/src/Bancalite.Persitence/Bancalite.Persitence.csproj ./src/Bancalite.Persitence/
COPY ./bancalite-backend/src/Bancalite.Application/Bancalite.Application.csproj ./src/Bancalite.Application/
COPY ./bancalite-backend/src/Bancalite.Infraestructure/Bancalite.Infraestructure.csproj ./src/Bancalite.Infraestructure/
COPY ./bancalite-backend/src/Bancalite.WebApi/Bancalite.WebApi.csproj ./src/Bancalite.WebApi/

# Restaura los paquetes NuGet de todos los proyectos referenciados.
RUN dotnet restore ./src/Bancalite.WebApi/Bancalite.WebApi.csproj

# Copia el resto del código fuente al contenedor.
COPY ./bancalite-backend/ ./

# Publica la aplicación en modo 'Release', optimizada para ejecución.
# El resultado se almacena en el directorio /app/publish.
# /p:UseAppHost=false crea un ejecutable dependiente del framework, más portable.
RUN dotnet publish ./src/Bancalite.WebApi/Bancalite.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# --- Etapa 2: Final Stage ---
# Se utiliza la imagen del SDK como base final para incluir las herramientas de la CLI de 'dotnet',
# como 'dotnet ef' para migraciones. Esto es ideal para entornos de desarrollo y pruebas.
# NOTA: Para producción, se recomienda usar la imagen 'aspnet:9.0' por ser más ligera y segura.
FROM mcr.microsoft.com/dotnet/sdk:9.0
WORKDIR /app

# Copia los artefactos publicados desde la etapa 'build' a la imagen final.
COPY --from=build /app/publish .

# Define el comando por defecto que se ejecutará al iniciar el contenedor.
ENTRYPOINT ["dotnet", "Bancalite.WebApi.dll"]