using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.DAL
{
    public interface IRepoPrenotazioni : IDisposable
    {
        Task<IEnumerable<Prenotazione?>> GetPrenotazioni();
        Task<IEnumerable<Prenotazione?>> GetPrenotazioni(string idUtente);
        Task<Prenotazione?> GetPrenotazione(int id);

        Task<bool> Insert(Prenotazione utente);
        Task<bool> Update(Prenotazione utente);
        Task<bool> Delete(Prenotazione utente);

        Task<int> Save();
    }
}
