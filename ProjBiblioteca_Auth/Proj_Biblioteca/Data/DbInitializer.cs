using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.Data
{
    public class DbInitializer
    {
        public static void Initialize(LibreriaContext context)
        {
            if (!context.Libri.Any())
            {

                var libri = new Libro[]
                {
                    new Libro {Titolo = "Il nome della rosa",          Autore = "Umberto Eco",             ISBN = 9788806202205, Disponibilita = 10, PrenotazioneMax = 10},
                    new Libro {Titolo = "1984",                        Autore = "George Orwell",           ISBN = 9788807811967, Disponibilita = 5,  PrenotazioneMax = 10},
                    new Libro {Titolo = "La coscienza di Zeno",        Autore = "Italo Svevo",             ISBN = 9788806210927, Disponibilita = 8,  PrenotazioneMax = 10},
                    new Libro {Titolo = "I promessi sposi",            Autore = "Alessandro Manzoni",      ISBN = 9788804604118, Disponibilita = 12, PrenotazioneMax = 10},
                    new Libro {Titolo = "Il piccolo principe",         Autore = "Antoine de Saint-Exupéry",ISBN = 9788884517882, Disponibilita = 15, PrenotazioneMax = 10},
                    new Libro {Titolo = "Don Chisciotte della Mancia", Autore = "Miguel de Cervantes",     ISBN = 9788871646820, Disponibilita = 6,  PrenotazioneMax = 10},
                    new Libro {Titolo = "Cent'anni di solitudine",     Autore = "Gabriel García Márquez",  ISBN = 9788806205695, Disponibilita = 4,  PrenotazioneMax = 10},
                    new Libro {Titolo = "Il gatto e il diavolo",       Autore = "Gianni Rodari",           ISBN = 9788804539845, Disponibilita = 18, PrenotazioneMax = 10},
                    new Libro {Titolo = "La divina commedia",          Autore = "Dante Alighieri",         ISBN = 9788804806790, Disponibilita = 7,  PrenotazioneMax = 10},
                    new Libro {Titolo = "Il Maestro e Margherita",     Autore = "Mikhail Bulgakov",        ISBN = 9788804662282, Disponibilita = 9,  PrenotazioneMax = 10}
                };

                context.Libri.AddRange(libri);
                context.SaveChanges();
            }

            if(!context.Utenti.Any())
            {
                var utenti = new Utente[]
                {
                    new Utente{Nome = "Admin", Email = "Admin@Admin.com", Password = "Admin", DDR = DateTime.Now, Ruolo = "Admin"}
                };

                context.Utenti.AddRange(utenti);
                context.SaveChanges();
            }

        }


    }
}
