using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using System.Data;
using System.Linq.Expressions;

namespace Proj_Biblioteca.DAL
{
    public interface IRepoPrenotazioni : IDisposable
    {
        Task<IEnumerable<Prenotazione?>> GetListAsync(int page, int pageSize);
        Task<IEnumerable<Prenotazione?>> GetListAsync(Expression<Func<Prenotazione, bool>> filters, int page, int pageSize);
        Task<IEnumerable<Prenotazione?>> GetListAsync(Expression<Func<Prenotazione, bool>> filters, Expression<Func<Prenotazione,object>> ordina, bool ordina_desc, int page, int pageSize);
        
        Task<Prenotazione?> GetAsync(Expression<Func<Prenotazione, bool>> filters);

        Task<int> PageCountAsync(int pageSize, Expression<Func<Prenotazione, bool>>? filters = null);

        Task<bool> Insert(Prenotazione utente);
        Task<bool> Update(Prenotazione utente);
        Task<bool> Delete(Prenotazione utente);

        Task<int> Save();
    }

    public class RepoPrenotazioni(LibreriaContext libreriaContext, ILogger<RepoPrenotazioni> logger) : IRepoPrenotazioni, IDisposable
    {
        private static readonly object _semaforo = new(); 
        private readonly LibreriaContext libreriaContext = libreriaContext;

        private readonly ILogger<RepoPrenotazioni> logger = logger;

        public async Task<IEnumerable<Prenotazione?>> GetListAsync(int page, int pageSize)
        {
            try
            {
                IQueryable<Prenotazione> query = libreriaContext.Prenotazioni.AsQueryable().AsNoTracking();

                int pageInt = Math.Max(page - 1, 0);
                int pageSizeInt = Math.Max(pageSize, 1);

                query = query.OrderBy(l => l.ID).Skip(pageInt * pageSizeInt).Take(pageSizeInt);


                return await query.Include(p => p.Libro).ToListAsync();
            }
            catch (OperationCanceledException canc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
            }
            catch (ArgumentNullException arg_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");

            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento delle prenotazioni: {ex.Message}");
            }
            return Enumerable.Empty<Prenotazione>();
        }

        public async Task<IEnumerable<Prenotazione?>> GetListAsync(Expression<Func<Prenotazione, bool>> filters, int page, int pageSize)
        {
            try
            {
                var query = libreriaContext.Prenotazioni.Include(p=>p.Libro).Include(p=>p.Utente).AsQueryable().AsNoTracking();

                query = query.Where(filters);

                int pageInt = Math.Max(page - 1 , 0);
                int pageSizeInt = Math.Max(pageSize, 1);

                query = query.OrderBy(l => l.ID).Skip(pageInt * pageSizeInt).Take(pageSizeInt);


                return await query.Select(p => new Prenotazione
                {
                    ID = p.ID,
                    Libro = p.Libro,
                    LibroID = p.LibroID,
                    DDF = p.DDF,
                    DDI = p.DDI,
                    UtenteId = p.UtenteId,
                    Utente = new()
                    {
                        Id = p.UtenteId ?? "",
                        Email = p.Utente!.Email,
                        DDR = p.Utente.DDR,
                        UserName = p.Utente.UserName,
                        PasswordHash = "",
                    },
                }).ToListAsync();
            }
            catch (OperationCanceledException canc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
            }
            catch (ArgumentNullException arg_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");

            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento delle prenotazioni: {ex.Message}");
            }
            return Enumerable.Empty<Prenotazione>();
        }

        public async Task<IEnumerable<Prenotazione?>> GetListAsync(Expression<Func<Prenotazione, bool>> filters, Expression<Func<Prenotazione, object>> ordina, bool ordina_desc, int page, int pageSize)
        {
            try
            {
                IQueryable<Prenotazione> query = libreriaContext.Prenotazioni.Include(p => p.Libro).Include(p => p.Utente).AsQueryable().AsNoTracking();

                query = query.Where(filters);

                int pageInt = Math.Max(page - 1, 0);
                int pageSizeInt = Math.Max(pageSize, 1);

                if (ordina_desc)
                    query = query.OrderByDescending(ordina).Skip(pageInt * pageSizeInt).Take(pageSizeInt);
                else
                    query = query.OrderBy(ordina).Skip(pageInt * pageSizeInt).Take(pageSizeInt);

                return await query.Select(p => new Prenotazione
                {
                    ID = p.ID,
                    Libro = p.Libro,
                    LibroID = p.LibroID,
                    DDF = p.DDF,
                    DDI = p.DDI,
                    UtenteId = p.UtenteId,
                    Utente = new()
                    {
                        Id = p.UtenteId??"",
                        Email = p.Utente!.Email,
                        DDR = p.Utente.DDR,
                        UserName = p.Utente.UserName,
                        PasswordHash = "",
                    },
                }).ToListAsync(); 
            }
            catch (OperationCanceledException canc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
            }
            catch (ArgumentNullException arg_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");

            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento delle prenotazioni: {ex.Message}");
            }
            return Enumerable.Empty<Prenotazione>();
        }

        public async Task<Prenotazione?> GetAsync(Expression<Func<Prenotazione, bool>> filters)
        {
            try
            {
                return await libreriaContext.Prenotazioni.Include(p=>p.Libro).AsNoTracking().FirstOrDefaultAsync(filters);
            }
            catch (OperationCanceledException canc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
            }
            catch (ArgumentNullException arg_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");

            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento delle prenotazioni: {ex.Message}");
            }
            return null;
        }

        public async Task<int> PageCountAsync(int pageSize, Expression<Func<Prenotazione, bool>>? filters = null)
        {
            try
            {
                pageSize = pageSize <= 0 ? 1 : pageSize;

                int totalCount = await libreriaContext.Prenotazioni.AsNoTracking().CountAsync(filters ?? (x => true));

                return (int)Math.Ceiling((double)totalCount / pageSize);
            }
            catch (OperationCanceledException canc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] l'operazione è stata annullata: {canc_ex.Message}");
            }
            catch (ArgumentNullException arg_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [ARGUMENT NULL EXCEPTION] Un parametro che non deve essere null è stato passato come null: {arg_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento delle prenotazioni: {ex.Message}");
            }
            return 0;
        }

        public async Task<bool> Delete(Prenotazione prenotazione)
        {
            if (prenotazione == null)
                return false;

            try
            {
                libreriaContext.Prenotazioni.Remove(prenotazione);

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
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nella rimozione della prenotazione: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio della rimozione della prenotazione: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nella rimozione della prenotazione: {ex.Message}");
            }
            return false;

        }

        public async Task<bool> Insert(Prenotazione prenotazione)
        {
            if (prenotazione == null)
                return false;

            try
            {
                Libro? libro = await libreriaContext.Libri.AsNoTracking().FirstOrDefaultAsync(l => l.ID == prenotazione.LibroID);
                if (libro == null)
                    return false;

                if (
                    prenotazione.DDI.AddDays(1) >= DateTime.UtcNow &&  // Prenotazione futura
                    prenotazione.DDI <= prenotazione.DDF &&  // Inizio è prima della fine
                    libro.Disponibilita > 0 &&  // Il libro è disponibile
                    prenotazione.DDF <= prenotazione.DDI.AddDays(libro.PrenotazioneMax + 1)  // Non supera il limite di giorni
                )
                {
                    lock (_semaforo)
                    {
                        bool prenotato = libreriaContext.Prenotazioni.Any(p => p.LibroID == prenotazione.LibroID && p.UtenteId == prenotazione.UtenteId);
                        if (prenotato) return false;

                        libreriaContext.Prenotazioni.Add(prenotazione);

                        if (Save().Result > 0)
                            return true;
                    }
                }

                return false;
            }
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di aggiunta cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nell' aggiunta della prenotazione: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio dell' aggiunta della prenotazione: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nell' aggiunta della prenotazione: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> Update(Prenotazione prenotazione)
        {
            if (prenotazione == null)
                return false;
            if (prenotazione.Libro == null)
                return false;

            if (
                prenotazione.DDI.AddDays(1) >= DateTime.UtcNow &&
                prenotazione.DDI <= prenotazione.DDF &&
                prenotazione.Libro.Disponibilita > 0 &&
                prenotazione.DDF <= prenotazione.DDI.AddDays(prenotazione.Libro.PrenotazioneMax + 1)
                )
            {
                try
                {
                    libreriaContext.Prenotazioni.Update(prenotazione);

                    if (await Save() > 0)
                        return true;
                }
                catch (OperationCanceledException opcanc_ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di update cancellata: {opcanc_ex.Message}");
                }
                catch (DbUpdateConcurrencyException updcc_ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nell' update della prenotazione: {updcc_ex.Message}");
                }
                catch (DbUpdateException upd_ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio dell' update della prenotazione: {upd_ex.Message}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nell' update della prenotazione: {ex.Message}");
                }
            }
            return false;

        }

        public async Task<int> Save()
        {
            try
            {
                return await libreriaContext.SaveChangesAsync();
            }
            catch (OperationCanceledException opcanc_ex)
            {
                throw new OperationCanceledException(opcanc_ex.Message);
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                throw new DbUpdateConcurrencyException(updcc_ex.Message);
            }
            catch (DbUpdateException upd_ex)
            {
                throw new DbUpdateException(upd_ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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
