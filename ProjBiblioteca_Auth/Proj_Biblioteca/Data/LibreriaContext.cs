using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.Data
{
    public class LibreriaContext : IdentityDbContext<Utente, Role, string>
    {

        public LibreriaContext(DbContextOptions<LibreriaContext> options) : base(options) 
        { 

        }

        public DbSet<Libro> Libri { get; set; }


        public DbSet<Prenotazione> Prenotazioni { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.UseCollation("Latin1_General_CS_AS");
            modelBuilder.Entity<Libro>().ToTable("Libro");
            modelBuilder.Entity<Prenotazione>().ToTable("Prenotazione");

            modelBuilder.Entity<Prenotazione>()
                        .HasOne<Utente>()          // La proprietà di navigazione
                        .WithMany()                     // Supponiamo che un Utente possa avere più Prenotazioni
                        .HasForeignKey(p => p.IdUtente) // La chiave esterna è IdUtente
                        .OnDelete(DeleteBehavior.Cascade); // Impostazione del comportamento in caso di eliminazione
        }
    }

    public class Role : IdentityRole<string>
    {

    }
}

