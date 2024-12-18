using Proj_Biblioteca.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_Biblioteca.Models
{
    public class Prenotazione
    {
        public int ID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DDI { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DDF { get; set; }

        public string? UtenteId { get; set; }
        public int LibroID { get; set; }

        public Utente? Utente { get; set; }
        public Libro? Libro { get; set; }

        [NotMapped]
        public UtenteViewModel? UtenteViewModel { get; set; }
    }
}
