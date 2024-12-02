using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_Biblioteca.Models
{
    public class Utente : IdentityUser<string>
    {


        [DataType(DataType.DateTime)]
        public DateTime DDR { get; set; }
    }
}
