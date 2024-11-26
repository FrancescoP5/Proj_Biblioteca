namespace Proj_Biblioteca.Models
{
    public class Libro
    {

        public int ID { get; set; }

        public string Titolo { get; set; }
        public string Autore { get; set; }
        public int PrenotazioneMax { get; set; }
        public long ISBN { get; set; }
        public int Disponibilita { get; set; }

    }
}
