namespace Proj_Biblioteca.ViewModels
{
    public class UtenteViewModel
    {
        public string? Id { get; set; }

        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Ruolo { get; set; }

        public DateTime DDR { get; set; }
    }
}
