using Proj_Biblioteca.Data;
using System.ComponentModel.DataAnnotations;

namespace Proj_Biblioteca.Models
{
    public class Utente : Entity
    {
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }

        public DateTime DDR { get; set; }

    }
}
