using Proj_Biblioteca.Data;

namespace Proj_Biblioteca.Models
{
    public class Libro : Entity
    {

        public string? Titolo { get; set; }
        public string? Autore { get; set; }

        private int _prenotazioneMax;
        public int PrenotazioneMax 
        {
            get => _prenotazioneMax;
            set
            {
                if(value < 0)
                {
                    _prenotazioneMax = 0;
                }
                else
                {
                    _prenotazioneMax = value;
                }
            }
        }

        private long _isbn;
        public long ISBN 
        { 
            get => _isbn;
            set
            {
                if (value < 0)
                {
                    _isbn = 0;
                }
                else
                {
                    _isbn = value;
                }
            }
        }

        private int _disponibilita;
        public int Disponibilita 
        {
            get => _disponibilita;
            set
            {
                if (value < 0)
                {
                    _disponibilita = 0;
                }
                else
                {
                    _disponibilita = value;
                }
            }
        }

    }
}
