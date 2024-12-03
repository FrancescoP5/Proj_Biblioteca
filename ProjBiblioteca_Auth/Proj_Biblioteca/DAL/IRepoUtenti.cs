using Microsoft.AspNetCore.Identity;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.DAL
{
    public interface IRepoUtenti : IDisposable
    {
        Task<IEnumerable<Utente?>> GetUtenti();
        Task<Utente?> GetUtente(string id);

        Task<IdentityUserRole<string>?> GetUserRole(string id);
        Task<Role?> GetRuolo(string id);

        Task<bool> CambiaRuolo(string id, string ruolo, UserManager<Utente> manager);

        Task<Utente?> Login(string email, string password);
        Task<bool> Registrazione(string nome, string email, string password, UserManager<Utente> manager);

        Task<bool> Insert(Utente utente);
        Task<bool> Update(Utente utente);
        Task<bool> Delete(Utente? utente);

        Task<int> Save();
    }
}
