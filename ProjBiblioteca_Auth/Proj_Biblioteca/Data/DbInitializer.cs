using Microsoft.AspNetCore.Identity;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Utils;

namespace Proj_Biblioteca.Data
{
    public class DbInitializer
    {
        public static void Initialize(LibreriaContext context, UserManager<Utente> userManager)
        {
            Dictionary<string, string> RoleIDs = new()
            {
                {"Admin",Guid.NewGuid().ToString()},
                {"Utente",Guid.NewGuid().ToString()},
            };

            if (!context.Libri.Any())
            {

                var libri = new Libro[]
                {
                    new() {Titolo = "Il nome della rosa",          Autore = "Umberto Eco",             ISBN = 9788806202205, Disponibilita = 10, PrenotazioneMax = 10},
                    new() {Titolo = "1984",                        Autore = "George Orwell",           ISBN = 9788807811967, Disponibilita = 5,  PrenotazioneMax = 10},
                    new() {Titolo = "La coscienza di Zeno",        Autore = "Italo Svevo",             ISBN = 9788806210927, Disponibilita = 8,  PrenotazioneMax = 10},
                    new() {Titolo = "I promessi sposi",            Autore = "Alessandro Manzoni",      ISBN = 9788804604118, Disponibilita = 12, PrenotazioneMax = 10},
                    new() {Titolo = "Il piccolo principe",         Autore = "Antoine de Saint-Exupéry",ISBN = 9788884517882, Disponibilita = 15, PrenotazioneMax = 10},
                    new() {Titolo = "Don Chisciotte della Mancia", Autore = "Miguel de Cervantes",     ISBN = 9788871646820, Disponibilita = 6,  PrenotazioneMax = 10},
                    new() {Titolo = "Cent'anni di solitudine",     Autore = "Gabriel García Márquez",  ISBN = 9788806205695, Disponibilita = 4,  PrenotazioneMax = 10},
                    new() {Titolo = "Il gatto e il diavolo",       Autore = "Gianni Rodari",           ISBN = 9788804539845, Disponibilita = 18, PrenotazioneMax = 10},
                    new() {Titolo = "La divina commedia",          Autore = "Dante Alighieri",         ISBN = 9788804806790, Disponibilita = 7,  PrenotazioneMax = 10},
                    new() {Titolo = "Il Maestro e Margherita",     Autore = "Mikhail Bulgakov",        ISBN = 9788804662282, Disponibilita = 9,  PrenotazioneMax = 10}
                };

                context.Libri.AddRange(libri);
                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                Role[] roles =
                [
                    new Role {Name = "Admin", NormalizedName="ADMIN",Id=RoleIDs["Admin"]},
                    new Role {Name = "Utente", NormalizedName="UTENTE",Id=RoleIDs["Utente"]}
                ];

                context.Roles.AddRange(roles);
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                Dictionary<string, string> UserIDs = new()
                {
                    {"AdminUser",Guid.NewGuid().ToString()},
                };

                Utente[] admins =
                [
                    new Utente {Id=UserIDs["AdminUser"], UserName="Admin", Email = "Admin@Admin.com", PasswordHash=Encryption.Encrypt("Admin"), SecurityStamp= Guid.NewGuid().ToString()}

                ];




                foreach (Utente admin in admins)
                {
                    userManager.CreateAsync(admin).Wait();
                }

                List<IdentityUserRole<string>> roles = [];

                foreach (Utente admin in admins)
                {
                    roles.Add(new IdentityUserRole<string> { RoleId = RoleIDs["Admin"], UserId = admin.Id });
                }

                context.UserRoles.AddRange(roles);

                context.SaveChanges();

            }

        }
    }
}
