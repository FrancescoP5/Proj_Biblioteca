using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
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

    public class RepoLibri(LibreriaContext libreriaContext) : IRepoLibri, IDisposable
    {

        private readonly LibreriaContext libreriaContext = libreriaContext;

        public async Task<IEnumerable<Libro?>> GetLibri()
        {
            try
            {
                return await libreriaContext.Libri.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Enumerable.Empty<Libro?>();
            }
        }

        public async Task<Libro?> GetLibro(int id)
        {
            try
            {
                return await libreriaContext.Libri.AsNoTracking().FirstOrDefaultAsync(l => l.ID == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<bool> Delete(Libro libro)
        {
            if (libro == null)
                return false;

            try
            {
                libreriaContext.Libri.Remove(libro);

                if (await Save() > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }

        public async Task<bool> Insert(Libro libro)
        {
            if (libro == null)
                return false;

            try
            {
                libreriaContext.Libri.Add(libro);

                if (await Save() > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }

        public async Task<bool> Update(Libro libro)
        {
            if (libro == null)
                return false;

            try
            {
                libreriaContext.Libri.Update(libro);

                if (await Save() > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }



        public async Task<int> Save()
        {
            try
            {
                return await libreriaContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }

        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            var success = false;
            if (!this.disposed)
            {
                if (disposing)
                {
                    ValueTask valueTask = libreriaContext.DisposeAsync();
                    success = valueTask.IsCompletedSuccessfully;
                }
            }
            if(success)
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
