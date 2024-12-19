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

    public class RepoLibri(LibreriaContext libreriaContext, ILogger<RepoLibri> logger) : IRepoLibri, IDisposable
    {
        private readonly LibreriaContext libreriaContext = libreriaContext;

        private readonly ILogger<RepoLibri> logger = logger;

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
            catch (OperationCanceledException canc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
                return Enumerable.Empty<Libro>();
            }
            catch (ArgumentNullException arg_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");
                return Enumerable.Empty<Libro>();

            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento dei libri: {ex.Message}");
                return Enumerable.Empty<Libro>();
            }
        }

        public async Task<Libro?> GetAsync(Expression<Func<Libro, bool>> filters)
        {
            try
            {
                return await libreriaContext.Libri.AsNoTracking().FirstOrDefaultAsync(filters);
            }
            catch (OperationCanceledException canc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
                return null;
            }
            catch (ArgumentNullException arg_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");
                return null;

            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento dei libri: {ex.Message}");
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
            catch (OperationCanceledException canc_ex){
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
            }
            catch (ArgumentNullException arg_ex){
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");
            }
            catch (Exception ex){
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento dei libri: {ex.Message}");
            }
                return 0;
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
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di rimozione cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nella rimozione del libro: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio della cancellazione del libro: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nella cancellazione del libro: {ex.Message}");
            }
            return false;
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
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di aggiunta cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nella aggiunta del libro: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio della aggiunta del libro: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nella aggiunta del libro: {ex.Message}");
            }
            return false;

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
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di update cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nell' update del libro: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio dell' update del libro: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nell' update del libro: {ex.Message}");
            }
            return false;

        }

        public async Task<int> Save()
        {
            try
            {
                return await libreriaContext.SaveChangesAsync();
            }
            catch (OperationCanceledException opcanc_ex) {
                throw new OperationCanceledException(opcanc_ex.Message);
            }
            catch (DbUpdateConcurrencyException updcc_ex){
                throw new DbUpdateConcurrencyException(updcc_ex.Message);
            }
            catch (DbUpdateException upd_ex){
                throw new DbUpdateException(upd_ex.Message);
            }
            catch (Exception ex){
                throw new Exception(ex.Message);
            }
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            var success = false;
            if (!disposed)
            {
                if (disposing)
                {
                    ValueTask valueTask = libreriaContext.DisposeAsync();
                    success = valueTask.IsCompletedSuccessfully;
                }
            }
            if(success)
            disposed = true;
        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (ArgumentNullException argnull_ex)
            {
                throw new ArgumentNullException(argnull_ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
