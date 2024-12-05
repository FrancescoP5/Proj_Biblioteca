using Proj_Biblioteca.Data;
using System.ComponentModel.DataAnnotations;

namespace Proj_Biblioteca.Models
{
    public class Utente : Entity
    {
        [DataType(DataType.DateTime)]
        public DateTime DDR { get; set; }

    }
}
