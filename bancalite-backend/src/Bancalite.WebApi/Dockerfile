# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files for restore layer caching
COPY ./bancalite-backend/src/Bancalite.Domain/Bancalite.Domain.csproj ./src/Bancalite.Domain/
COPY ./bancalite-backend/src/Bancalite.Persitence/Bancalite.Persitence.csproj ./src/Bancalite.Persitence/
COPY ./bancalite-backend/src/Bancalite.Application/Bancalite.Application.csproj ./src/Bancalite.Application/
COPY ./bancalite-backend/src/Bancalite.Infraestructure/Bancalite.Infraestructure.csproj ./src/Bancalite.Infraestructure/
COPY ./bancalite-backend/src/Bancalite.WebApi/Bancalite.WebApi.csproj ./src/Bancalite.WebApi/

RUN dotnet restore ./src/Bancalite.WebApi/Bancalite.WebApi.csproj

# Copy the rest of the source and publish
COPY ./bancalite-backend/ ./
RUN dotnet publish ./src/Bancalite.WebApi/Bancalite.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Install curl for container healthchecks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bancalite.WebApi.dll"]

