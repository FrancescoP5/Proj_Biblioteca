using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.Data
{
    public class LibreriaContext : DbContext
    {

        public LibreriaContext(DbContextOptions<LibreriaContext> options) : base(options) 
        { 

        }

        public DbSet<Libro> Libri { get; set; }

        public DbSet<Utente> Utenti { get; set; }

        public DbSet<Prenotazione> Prenotazioni { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Libro>().ToTable("Libro");
            modelBuilder.Entity<Utente>().ToTable("Utente");
            modelBuilder.Entity<Prenotazione>().ToTable("Prenotazione");

        }
    }
}
