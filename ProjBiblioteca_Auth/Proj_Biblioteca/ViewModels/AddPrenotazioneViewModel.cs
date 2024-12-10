namespace Proj_Biblioteca.ViewModels
{
    public class AddPrenotazioneViewModel
    {
        public int? IdLibro { get; set; }
        public string? IdUtente { get; set; }
        public string? Inizio { get; set; }
        public string? Fine { get; set; }
        public int TimeOffset { get; set; }
        public int ClientTime { get; set; }
    }
}
