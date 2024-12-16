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
                    new() {Titolo = "Il nome della rosa",           Autore = "Umberto Eco",                ISBN = 9788806202205, Disponibilita = 10, PrenotazioneMax = 10},
                    new() {Titolo = "1984",                         Autore = "George Orwell",              ISBN = 9788807811967, Disponibilita = 20,  PrenotazioneMax = 16},
                    new() {Titolo = "La coscienza di Zeno",         Autore = "Italo Svevo",                ISBN = 9788806210927, Disponibilita = 30,  PrenotazioneMax = 20},
                    new() {Titolo = "I promessi sposi",             Autore = "Alessandro Manzoni",         ISBN = 9788804604118, Disponibilita = 12, PrenotazioneMax = 4},
                    new() {Titolo = "Il piccolo principe",          Autore = "Antoine de Saint-Exupéry",   ISBN = 9788884517882, Disponibilita = 15, PrenotazioneMax = 6},
                    new() {Titolo = "Don Chisciotte della Mancia",  Autore = "Miguel de Cervantes",        ISBN = 9788871646820, Disponibilita = 6,  PrenotazioneMax = 12},
                    new() {Titolo = "Cent'anni di solitudine",      Autore = "Gabriel García Márquez",     ISBN = 9788806205695, Disponibilita = 40,  PrenotazioneMax = 100},
                    new() {Titolo = "Il gatto e il diavolo",        Autore = "Gianni Rodari",              ISBN = 9788804539845, Disponibilita = 18, PrenotazioneMax = 20},
                    new() {Titolo = "La divina commedia",           Autore = "Dante Alighieri",            ISBN = 9788804806790, Disponibilita = 7,  PrenotazioneMax = 30},
                    new() {Titolo = "Il Maestro e Margherita",      Autore = "Mikhail Bulgakov",           ISBN = 9788804662282, Disponibilita = 13,  PrenotazioneMax = 50},
                    new() {Titolo = "Orgoglio e pregiudizio",       Autore = "Jane Austen",                ISBN = 9780141199078, Disponibilita = 10, PrenotazioneMax = 54},
                    new() {Titolo = "Il ritratto di Dorian Gray",   Autore = "Oscar Wilde",                ISBN = 9780141439570, Disponibilita = 8,  PrenotazioneMax = 12},
                    new() {Titolo = "Moby Dick",                    Autore = "Herman Melville",            ISBN = 9781853260087, Disponibilita = 6,  PrenotazioneMax = 10},
                    new() {Titolo = "La montagna incantata",        Autore = "Thomas Mann",                ISBN = 9788804479392, Disponibilita = 12, PrenotazioneMax = 14},
                    new() {Titolo = "La strada",                    Autore = "Cormac McCarthy",            ISBN = 9788806181054, Disponibilita = 4,  PrenotazioneMax = 23},
                    new() {Titolo = "Ulisse",                       Autore = "James Joyce",                ISBN = 9780141182803, Disponibilita = 16,  PrenotazioneMax = 40},
                    new() {Titolo = "Frankenstein",                 Autore = "Mary Shelley",               ISBN = 9780141439471, Disponibilita = 20,  PrenotazioneMax = 56},
                    new() {Titolo = "Il giovane Holden",            Autore = "J.D. Salinger",              ISBN = 9780316769488, Disponibilita = 10, PrenotazioneMax = 25},
                    new() {Titolo = "Il grande Gatsby",             Autore = "F. Scott Fitzgerald",        ISBN = 9780743273565, Disponibilita = 9,  PrenotazioneMax = 5},
                    new() {Titolo = "La metamorfosi",               Autore = "Franz Kafka",                ISBN = 9780141181943, Disponibilita = 15, PrenotazioneMax = 57},
                    new() {Titolo = "Cime tempestose",              Autore = "Emily Brontë",               ISBN = 9780141439556, Disponibilita = 30,  PrenotazioneMax = 75},
                    new() {Titolo = "Il conte di Montecristo",      Autore = "Alexandre Dumas",            ISBN = 9781853267338, Disponibilita = 18, PrenotazioneMax = 45},
                    new() {Titolo = "Il piccolo principe",          Autore = "Antoine de Saint-Exupéry",   ISBN = 9788804657554, Disponibilita = 20, PrenotazioneMax = 25},
                    new() {Titolo = "Le affinità elettive",         Autore = "Johann Wolfgang von Goethe", ISBN = 9788817184864, Disponibilita = 19,  PrenotazioneMax = 18},
                    new() {Titolo = "La ricerca del tempo perduto", Autore = "Marcel Proust",              ISBN = 9780141180335, Disponibilita = 4,  PrenotazioneMax = 15},
                    new() {Titolo = "La casa degli spiriti",        Autore = "Isabel Allende",             ISBN = 9788807883056, Disponibilita = 10, PrenotazioneMax = 7},
                    new() {Titolo = "I viaggi di Gulliver",         Autore = "Jonathan Swift",             ISBN = 9780140436226, Disponibilita = 6,  PrenotazioneMax = 9},
                    new() {Titolo = "La nube purpurea",             Autore = "M.P. Shiel",                 ISBN = 9780994608321, Disponibilita = 9,  PrenotazioneMax = 11},
                    new() {Titolo = "Il mondo nuovo",               Autore = "Aldous Huxley",              ISBN = 9780141187662, Disponibilita = 12, PrenotazioneMax = 15},
                    new() {Titolo = "I fratelli Karamazov",         Autore = "Fëdor Dostoevskij",          ISBN = 9780140449242, Disponibilita = 7,  PrenotazioneMax = 16}
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
                    new Utente {Id=UserIDs["AdminUser"], UserName="Admin", Email = "Admin@Admin.com", PasswordHash="Admin", SecurityStamp= Guid.NewGuid().ToString()}

                ];




                foreach (Utente admin in admins)
                {
                    admin.PasswordHash = Encryption.HashPassword(admin.PasswordHash??"Admin", admin);
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
