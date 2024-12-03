using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.DAL
{
    public interface IRepoLibri : IDisposable
    {
        Task<IEnumerable<Libro?>> GetLibri();
        Task<Libro?> GetLibro(int id);

        Task<bool> Insert(Libro libro);
        Task<bool> Update(Libro libro);
        Task<bool> Delete(Libro libro);

        Task<int> Save();
    }
}
