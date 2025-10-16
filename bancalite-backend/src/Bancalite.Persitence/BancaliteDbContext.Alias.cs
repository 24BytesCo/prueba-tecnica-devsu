using System;
using Microsoft.EntityFrameworkCore;

namespace Bancalite.Persitence
{
    // Alias para compatibilidad con migraciones existentes
    public class BancaliteDbContext : BancaliteContext
    {
        public BancaliteDbContext(DbContextOptions<BancaliteContext> options)
            : base(options)
        {
        }
    }
}

