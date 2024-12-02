using Proj_Biblioteca.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_Biblioteca.Models
{
    public class Prenotazione
    {
        public int ID { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DDI {  get; set; }
        [DataType(DataType.DateTime)]
        public DateTime DDF {  get; set; }

        [ForeignKey("Utente")]
        public string? IdUtente { get; set; }
        public int LibroID { get; set; }

        public Utente? Utente { private get;  set; }
        public Libro? Libro { get; set; }

        [NotMapped]
        public UtenteViewModel? UtenteViewModel { get; set; }


    }
}
