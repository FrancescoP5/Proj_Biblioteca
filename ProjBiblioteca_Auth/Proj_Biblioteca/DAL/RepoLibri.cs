using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using System.Linq.Expressions;

namespace Proj_Biblioteca.DAL
{
    public interface IRepoLibri : IDisposable
    {
        Task<IEnumerable<Libro?>> GetListAsync(Expression<Func<Libro, bool>>? filters = null, int? page = null, int? pageSize = null);
        Task<Libro?> GetAsync(Expression<Func<Libro, bool>> filters);

        Task<int> PageCountAsync(int pageSize, Expression<Func<Libro, bool>>? filters = null);

        Task<bool> Insert(Libro libro);
        Task<bool> Update(Libro libro);
        Task<bool> Delete(Libro libro);
        
        Task<int> Save();
    }

    public class RepoLibri(LibreriaContext libreriaContext) : IRepoLibri, IDisposable
    {
        private readonly LibreriaContext libreriaContext = libreriaContext;

        public async Task<IEnumerable<Libro?>> GetListAsync(Expression<Func<Libro, bool>>? filters = null, int? page = null, int? pageSize = null)
        {
            try
            {
                IQueryable<Libro> query = libreriaContext.Libri.AsQueryable().AsNoTracking();

                if (filters != null)
                    query = query.Where(filters);

                if (page.HasValue && pageSize.HasValue)
                {
                    int pageInt = Math.Max(page.Value - 1, 0);
                    int pageSizeInt = Math.Max(pageSize.Value, 1);

                    query = query.OrderBy(l => l.ID).Skip(pageInt * pageSizeInt).Take(pageSizeInt);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il caricamento dei libri: {ex.Message}");
                return Enumerable.Empty<Libro>();
            }
        }

        public async Task<Libro?> GetAsync(Expression<Func<Libro, bool>> filters)
        {
            try
            {
                return await libreriaContext.Libri.AsNoTracking().FirstOrDefaultAsync(filters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il caricamento del libro: {ex.Message}");
                return null;
            }
        }

        public async Task<int> PageCountAsync(int pageSize, Expression<Func<Libro, bool>>? filters = null)
        {
            try
            {
                pageSize = pageSize <= 0 ? 1 : pageSize;

                int totalCount = await libreriaContext.Libri.AsNoTracking().CountAsync(filters ?? (x => true));
                
                return (int) Math.Ceiling((double)totalCount / pageSize); 
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Errore durante il caricamento dei libri: {ex.Message}");
                return 0;
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
