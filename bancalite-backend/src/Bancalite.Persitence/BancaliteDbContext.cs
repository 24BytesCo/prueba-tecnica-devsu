using System;
using System.Linq;
using System.IO;
using System.Text.Json;
using Bancalite.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Bancalite.Persitence.Model;
using Microsoft.AspNetCore.Identity;

namespace Bancalite.Persitence
{
    public class BancaliteContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public BancaliteContext(DbContextOptions<BancaliteContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Cuenta> Cuentas { get; set; } = null!;
        public DbSet<Movimiento> Movimientos { get; set; } = null!;
        public DbSet<Persona> Personas { get; set; } = null!;
        public DbSet<TipoCuenta> TiposCuenta { get; set; } = null!;
        public DbSet<TipoDocumentoIdentidad> TiposDocumentoIdentidad { get; set; } = null!;
        public DbSet<TipoMovimiento> TiposMovimiento { get; set; } = null!;
        public DbSet<Genero> Generos { get; set; } = null!;
        
        // Configuración de la conexión (Postgres)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Solo configurar si no viene configurado por DI (mejor práctica)
            if (!optionsBuilder.IsConfigured)
            {
                var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
                if (string.IsNullOrWhiteSpace(conn))
                {
                    var host = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
                    var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
                    var db   = Environment.GetEnvironmentVariable("DB_NAME") ?? "bancalite";
                    var user = Environment.GetEnvironmentVariable("DB_USER") ?? "admin";
                    var pass = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;

                    conn = $"Host={host};Port={port};Database={db};Username={user};Password={pass};Pooling=true";
                }
                optionsBuilder
                    .UseNpgsql(conn)
                    .LogTo(Console.WriteLine,
                        [DbLoggerCategory.Database.Command.Name], Microsoft.Extensions.Logging.LogLevel.Information)
                    .EnableSensitiveDataLogging();
            }
        }

        // Configuración de entidades y relaciones
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Nombres de tabla snake_case plural
            modelBuilder.Entity<Persona>().ToTable("personas");
            modelBuilder.Entity<Cliente>().ToTable("clientes");
            modelBuilder.Entity<Cuenta>().ToTable("cuentas");
            modelBuilder.Entity<Movimiento>().ToTable("movimientos");
            modelBuilder.Entity<Genero>().ToTable("generos");
            // Tabla de roles propia del dominio eliminada en favor de Identity
            modelBuilder.Entity<TipoCuenta>().ToTable("tipos_cuenta");
            modelBuilder.Entity<TipoMovimiento>().ToTable("tipos_movimiento");
            modelBuilder.Entity<TipoDocumentoIdentidad>().ToTable("tipos_documento_identidad");

            // BaseEntity: PK y timestamps
            foreach (var et in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            {
                modelBuilder.Entity(et.ClrType).HasKey("Id");
                modelBuilder.Entity(et.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                modelBuilder.Entity(et.ClrType)
                    .Property<DateTime?>("UpdatedAt");
            }

            // Catálogos
            modelBuilder.Entity<Genero>(e =>
            {
                e.Property(x => x.Codigo).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.Codigo).IsUnique();
                e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
                e.Property(x => x.Activo).HasDefaultValue(true);
            });

            modelBuilder.Entity<TipoDocumentoIdentidad>(e =>
            {
                e.Property(x => x.Codigo).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.Codigo).IsUnique();
                e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
                e.Property(x => x.Activo).HasDefaultValue(true);
            });

            modelBuilder.Entity<TipoCuenta>(e =>
            {
                e.Property(x => x.Codigo).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.Codigo).IsUnique();
                e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
                e.Property(x => x.Activo).HasDefaultValue(true);
            });

            modelBuilder.Entity<TipoMovimiento>(e =>
            {
                e.Property(x => x.Codigo).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.Codigo).IsUnique();
                e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
                e.Property(x => x.Activo).HasDefaultValue(true);
                e.Property(x => x.CreatedBy).HasMaxLength(100);
            });

            // Persona
            modelBuilder.Entity<Persona>(e =>
            {
                e.Property(x => x.Nombres).IsRequired().HasMaxLength(120);
                e.Property(x => x.Apellidos).IsRequired().HasMaxLength(120);
                e.Property(x => x.NumeroDocumento).IsRequired().HasMaxLength(50);
                e.Property(x => x.Direccion).HasMaxLength(200);
                e.Property(x => x.Telefono).HasMaxLength(50);
                e.Property(x => x.Email).HasMaxLength(200);

                e.HasIndex(x => new { x.TipoDocumentoIdentidadId, x.NumeroDocumento }).IsUnique();

                e.HasOne(x => x.Genero)
                    .WithMany(g => g.Personas)
                    .HasForeignKey(x => x.GeneroId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.TipoDocumentoIdentidad)
                    .WithMany(t => t.Personas)
                    .HasForeignKey(x => x.TipoDocumentoIdentidadId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cliente 1–1 Persona
            modelBuilder.Entity<Cliente>(e =>
            {
                e.Property(x => x.PasswordHash).HasMaxLength(256);
                e.Property(x => x.Estado).HasDefaultValue(true);

                e.HasIndex(x => x.PersonaId).IsUnique();

                e.HasOne(x => x.Persona)
                    .WithOne(p => p.Cliente)
                    .HasForeignKey<Cliente>(x => x.PersonaId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Rol de autorización se maneja con Identity; opcionalmente indexamos AppUserId
                e.HasIndex(x => x.AppUserId).IsUnique(false);
            });

            // Cuenta
            modelBuilder.Entity<Cuenta>(e =>
            {
                e.Property(x => x.NumeroCuenta).IsRequired().HasMaxLength(30);
                e.HasIndex(x => x.NumeroCuenta).IsUnique();

                e.Property(x => x.SaldoInicial).HasPrecision(18, 2);
                e.Property(x => x.SaldoActual).HasPrecision(18, 2);

                e.Property(x => x.Estado)
                    .HasConversion<string>()
                    .HasMaxLength(12);

                e.HasOne(x => x.Cliente)
                    .WithMany(c => c.Cuentas)
                    .HasForeignKey(x => x.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.TipoCuenta)
                    .WithMany(tc => tc.Cuentas)
                    .HasForeignKey(x => x.TipoCuentaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Movimiento
            modelBuilder.Entity<Movimiento>(e =>
            {
                e.Property(x => x.Monto).HasPrecision(18, 2);
                e.Property(x => x.SaldoPrevio).HasPrecision(18, 2);
                e.Property(x => x.SaldoPosterior).HasPrecision(18, 2);
                e.Property(x => x.Descripcion).HasMaxLength(250);
                e.Property(x => x.CreatedBy).HasMaxLength(100);

                e.HasOne(x => x.Cuenta)
                    .WithMany(c => c.Movimientos)
                    .HasForeignKey(x => x.CuentaId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.Tipo)
                    .WithMany(tm => tm.Movimientos)
                    .HasForeignKey(x => x.TipoId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => new { x.CuentaId, x.Fecha });
            });
        }

        /// <summary>
        /// Inicializa catálogos desde archivos JSON si las tablas están vacías.
        /// </summary>
        /// <remarks>
        /// Busca archivos en la carpeta 'SeedData' ubicada junto al ejecutable.
        /// Solo inserta datos cuando no existen registros previos.
        /// </remarks>
        /// <param name="ct">Token de cancelación opcional.</param>
        public async Task SeedCatalogosAsync(CancellationToken ct = default)
        {
            // Generos
            if (!await Generos.AsNoTracking().AnyAsync(ct))
            {
                var data = ReadSeed<Genero>("generos.json");
                foreach (var item in data)
                {
                    if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
                }
                if (data.Length > 0) Generos.AddRange(data);
            }

            // Tipos de Documento
            if (!await TiposDocumentoIdentidad.AsNoTracking().AnyAsync(ct))
            {
                var data = ReadSeed<TipoDocumentoIdentidad>("tipos_documento_identidad.json");
                foreach (var item in data)
                {
                    if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
                }
                if (data.Length > 0) TiposDocumentoIdentidad.AddRange(data);
            }

            // Tipos de Cuenta
            if (!await TiposCuenta.AsNoTracking().AnyAsync(ct))
            {
                // Tipos de Cuenta
                var data = ReadSeed<TipoCuenta>("tipos_cuenta.json");
                foreach (var item in data)
                {
                    if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
                }
                if (data.Length > 0) TiposCuenta.AddRange(data);
            }

            // Tipos de Movimiento
            if (!await TiposMovimiento.AsNoTracking().AnyAsync(ct))
            {
                var data = ReadSeed<TipoMovimiento>("tipos_movimiento.json");
                foreach (var item in data)
                {
                    if (item.Id == Guid.Empty) item.Id = Guid.NewGuid();
                }
                if (data.Length > 0) TiposMovimiento.AddRange(data);
            }

            // Roles: se siembran con RoleManager de Identity, no en este contexto

            await SaveChangesAsync(ct);
        }

        /// <summary>
        /// Lee y deserializa un archivo JSON de la carpeta 'SeedData'.
        /// </summary>
        private static T[] ReadSeed<T>(string fileName)
        {
            try
            {
                // Construir ruta del archivo
                var path = Path.Combine(AppContext.BaseDirectory, "SeedData", fileName);
                if (!File.Exists(path)) return [];

                // Leer y deserializar
                var json = File.ReadAllText(path);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Ignorar valores nulos en el JSON
                return JsonSerializer.Deserialize<T[]>(json, options) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}
