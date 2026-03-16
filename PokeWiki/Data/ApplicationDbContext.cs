using Microsoft.EntityFrameworkCore;
using PokeWiki.Data.Entities.PokeWiki.Web.Data.Entities;
using PokeWiki.Web.Data.Entities;
using PokeWiki.Web.Data.Views;

namespace PokeWiki.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioAuxiliar> Usuarios_Auxiliar { get; set; }

        public DbSet<PokemonBiologiaView> VistaPokemonBiologia { get; set; }
        public DbSet<EvolucionCrianzaView> VistaEvolucionCrianza { get; set; }
        public DbSet<MovimientosView> VistaMovimientos { get; set; }
        public DbSet<PokemonMove> PokemonMoves { get; set; }
        public DbSet<PokemonMoveMethod> PokemonMoveMethods { get; set; }
        public DbSet<PokemonEvolution> PokemonEvolutions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<PokemonMove>(e => e.HasNoKey());
            builder.Entity<PokemonMoveMethod>(e => e.HasKey(m => m.id));

            builder.Entity<Usuario>()
                .HasOne(u => u.UsuarioAuxiliar)
                .WithOne(ua => ua.Usuario)
                .HasForeignKey<UsuarioAuxiliar>(ua => ua.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PokemonBiologiaView>(e => {
                e.HasNoKey();
                e.ToView("Vista_TODO_Pokemon_Biologia");
            });

            builder.Entity<EvolucionCrianzaView>(e => {
                e.HasNoKey();
                e.ToView("Vista_TODO_Evolucion_Crianza");
            });

            builder.Entity<MovimientosView>(e => {
                e.HasNoKey();
                e.ToView("Vista_TODO_Movimientos");
            });
        }
    }
}