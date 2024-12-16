using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.Data
{
    public class LibreriaContext(DbContextOptions<LibreriaContext> options) : IdentityDbContext<Utente, Role, string>(options)
    {
        public DbSet<Libro> Libri { get; set; }


        public DbSet<Prenotazione> Prenotazioni { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.UseCollation("Latin1_General_CS_AS");
            modelBuilder.Entity<Libro>().ToTable("Libro");
            modelBuilder.Entity<Prenotazione>().ToTable("Prenotazione");
        }
    }

    public class Role : IdentityRole<string>
    {

    }
}

