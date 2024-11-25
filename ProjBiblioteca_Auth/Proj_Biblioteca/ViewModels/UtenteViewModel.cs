using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;

namespace Proj_Biblioteca.ViewModels
{
    public class UtenteViewModel
    {
        public int ID { get; set; }

        public string Nome { get; set; }
        public string Email { get; set; }
        public string Ruolo { get; set; }

        public DateTime DDR { get; set; }


        public static async Task<UtenteViewModel> GetViewModel(LibreriaContext _libreria, int? id)
        {

            if (id == null)
                return null;

            var user = await _libreria.Utenti.AsNoTracking().FirstOrDefaultAsync(u => u.ID == id);
            return new UtenteViewModel()
            {
                ID = user.ID,
                DDR = user.DDR,
                Email = user.Email,
                Nome = user.Nome,
                Ruolo = user.Ruolo
            };
        }

        public static async Task<List<UtenteViewModel>> GetViewModel(LibreriaContext _libreria)
        {
            var users = await _libreria.Utenti.AsNoTracking().ToListAsync();

            List<UtenteViewModel> utentiViewModel = new List<UtenteViewModel>();
            foreach (var user in users)
            {
                utentiViewModel.Add(new UtenteViewModel()
                {
                    ID = user.ID,
                    DDR = user.DDR,
                    Email = user.Email,
                    Nome = user.Nome,
                    Ruolo = user.Ruolo
                });
            }

            return utentiViewModel;
        }

    }
}
