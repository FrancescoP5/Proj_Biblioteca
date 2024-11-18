using Proj_Biblioteca.Data;
using System.ComponentModel.DataAnnotations;

namespace Proj_Biblioteca.Models
{
    public class Prenotazione : Entity
    {
        private DateTime _ddi;
        public DateTime DDI 
        {
            get => _ddi; 
            set
            {
                if (value.Subtract(DateTime.Now).TotalDays > -2)
                {
                    _ddi = value;
                }
                else
                {
                    _ddi = DateTime.Now;
                }
            }
        }

        private DateTime _ddf;
        public DateTime DDF 
        {
            get => _ddf; 
            set
            {
                if(value < DDI)
                {
                    _ddf = DDI;
                }
                else
                {
                    _ddf = value;
                }
            }
        }

        [Required]
        public Utente Utente { get; set; }
        [Required]
        public Libro Libro { get; set; }
    }
}
