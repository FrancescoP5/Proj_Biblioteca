using Proj_Biblioteca.Data;
using System.ComponentModel.DataAnnotations;

namespace Proj_Biblioteca.Models
{
    public class Prenotazione
    {
        public int ID { get; set; }

        public DateTime DDI {  get; set; }
        public DateTime DDF {  get; set; }

        public int UtenteID { get; set; }
        public int LibroID { get; set; }

        public Utente Utente { get; set; }
        public Libro Libro { get; set; }
    }
}
