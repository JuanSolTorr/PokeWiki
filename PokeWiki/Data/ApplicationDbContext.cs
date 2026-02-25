using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokeWiki.Web.Data.Views;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace PokeWiki.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PokemonBiologiaView> VistaPokemonBiologia { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración obligatoria para Vistas SQL
            builder.Entity<PokemonBiologiaView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("Vista_TODO_Pokemon_Biologia");
            });
        }
    }
}