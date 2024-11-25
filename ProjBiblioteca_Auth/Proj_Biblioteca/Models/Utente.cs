using Proj_Biblioteca.Data;
using System.ComponentModel.DataAnnotations;

namespace Proj_Biblioteca.Models
{
    public class Utente
    {
        public int ID { get; set; }

        public string Nome { get; set; }
        public string Email { get; set; }
        public string Ruolo { get; set; }
        public DateTime DDR { get; set; }

    }
}
