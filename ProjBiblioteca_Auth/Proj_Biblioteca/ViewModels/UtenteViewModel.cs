using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;

namespace Proj_Biblioteca.ViewModels
{
    public class UtenteViewModel
    {
        public string? Id { get; set; }

        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Ruolo { get; set; }

        public DateTime DDR { get; set; }


        public static async Task<UtenteViewModel?> GetViewModel(LibreriaContext _libreria, string id)
        {
            try
            {

                if (id == null)
                    return null;

                var user = await _libreria.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                var ruoloID = (await _libreria.UserRoles.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id))?.RoleId;
                var ruolo = (await _libreria.Roles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == ruoloID))?.Name;

                if (user == null)
                    return null;

                return new UtenteViewModel()
                {
                    Id = user.Id,
                    DDR = user.DDR,
                    Email = user.Email ?? "null",
                    Nome = user.UserName ?? "null",
                    Ruolo = ruolo ?? "Utente"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public static async Task<List<UtenteViewModel>> GetViewModel(LibreriaContext _libreria)
        {
            try
            {
                var users = await _libreria.Users.AsNoTracking().ToListAsync();

                List<UtenteViewModel> utentiViewModel = new List<UtenteViewModel>();
                foreach (var user in users)
                {

                    var ruoloID = (await _libreria.UserRoles.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == user.Id))?.RoleId;
                    var ruolo = (await _libreria.Roles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == ruoloID))?.Name;

                    utentiViewModel.Add(new UtenteViewModel()
                    {
                        Id = user.Id,
                        DDR = user.DDR,
                        Email = user.Email ?? "null",
                        Nome = user.UserName ?? "null",
                        Ruolo = ruolo ?? "Utente"
                    });
                }

                return utentiViewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return [];
        }

    }
}
