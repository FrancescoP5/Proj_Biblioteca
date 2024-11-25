using Proj_Biblioteca.Data;
using Proj_Biblioteca.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Proj_Biblioteca.Models
{
    public class Prenotazione
    {
        public int ID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DDI {  get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DDF {  get; set; }

        public int UtenteID { get; set; }
        public int LibroID { get; set; }

        public UtenteViewModel Utente { get; set; }
        public Libro Libro { get; set; }
    }
}
