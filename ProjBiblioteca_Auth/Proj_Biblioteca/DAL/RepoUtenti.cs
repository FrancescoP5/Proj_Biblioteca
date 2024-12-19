using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Utils;

namespace Proj_Biblioteca.DAL
{
    public interface IRepoUtenti : IDisposable
    {
        Task<IEnumerable<Utente?>> GetUtenti();
        Task<Utente?> GetUtente(string id);

        Task<IdentityUserRole<string>?> GetUserRole(string id);
        Task<Role?> GetRuolo(string id);

        Task<bool> CambiaRuolo(string id, string ruolo);

        Task<Utente?> GetByCredentials(string email, string password);
        Task<bool> InsertByCredentials(string nome, string email, string password);

        Task<bool> Insert(Utente utente);
        Task<bool> Update(Utente utente);
        Task<bool> Delete(Utente? utente);

        Task<bool> Login(Utente utente);
        Task<bool> Logout();

        Task<int> Save();
    }

    public class RepoUtenti(LibreriaContext libreriaContext, SignInManager<Utente> signInManager, UserManager<Utente> userManager, ILogger<RepoUtenti> logger) : IRepoUtenti, IDisposable
    {

        private readonly LibreriaContext libreriaContext = libreriaContext;
        private readonly SignInManager<Utente> signInManager = signInManager;
        private readonly UserManager<Utente> userManager = userManager;

        private readonly ILogger<RepoUtenti> logger = logger;

        public async Task<IEnumerable<Utente?>> GetUtenti()
        {
            try
            {
                return await libreriaContext.Users.AsNoTracking().ToListAsync();
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
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento degli utenti: {ex.Message}");
            }
            return Enumerable.Empty<Utente>();
        }

        public async Task<Utente?> GetUtente(string id)
        {
            try
            {
                return await libreriaContext.Users.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
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
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento degli utenti: {ex.Message}");
            }
            return null;
        }

        public async Task<IdentityUserRole<string>?> GetUserRole(string id)
        {
            try
            {
                return await libreriaContext.UserRoles.AsNoTracking().FirstOrDefaultAsync(l => l.UserId == id);
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
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento degli utenti: {ex.Message}");
            }
            return null;
        }

        public async Task<Role?> GetRuolo(string id)
        {
            try
            {
                return await libreriaContext.Roles.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
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
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento degli utenti: {ex.Message}");
            }
            return null;
        }

        public async Task<Utente?> GetByCredentials(string email, string passwordHash)
        {
            try
            {
                Utente? utente = await libreriaContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
                if (utente == null) return null;

                var verificaPassword = Encryption.VerifyPassword(passwordHash, utente);

                if(verificaPassword == "Verificato") return utente;
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
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore durante il caricamento degli utenti: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> InsertByCredentials(string nome, string email, string password)
        {
            Utente utente = new() { Id = Guid.NewGuid().ToString(), UserName = nome, Email = email, PasswordHash = password, DDR = DateTime.UtcNow };
            utente.PasswordHash = Encryption.HashPassword(password, utente);
            try
            {
                IdentityResult registerResult = await userManager.CreateAsync(utente);

                if (registerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(utente, "Utente");
                    return true;
                }
            }
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di inserimento cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nel inserimento del utente: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio del inserimento del ruolo: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nel inserimento del ruolo: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> CambiaRuolo(string id, string ruolo)
        {
            var oldRoleID = (await GetUserRole(id))?.RoleId;
            var oldRole = (await GetRuolo(oldRoleID ?? "0"))?.Name;
            Utente? utente = await libreriaContext.FindAsync<Utente>(id);
            if (oldRole != ruolo)
            {
                if (utente == null)
                    return false;

                try
                {
                    await userManager.RemoveFromRoleAsync(utente, oldRole ?? "Utente");
                    await userManager.AddToRoleAsync(utente, ruolo);
                    await userManager.UpdateSecurityStampAsync(utente);

                    return true;
                }
                catch (OperationCanceledException opcanc_ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di cambiamento ruolo cancellata: {opcanc_ex.Message}");
                }
                catch (DbUpdateConcurrencyException updcc_ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nel cambiamento del utente: {updcc_ex.Message}");
                }
                catch (DbUpdateException upd_ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio del cambiamento del ruolo: {upd_ex.Message}");
                }
                catch (Exception ex)
                {
                    logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nel cambiamento del ruolo: {ex.Message}");
                }
                return false;
            }
            return true;
        }

        public async Task<bool> Delete(Utente? utente)
        {
            if (utente == null)
                return false;

            try
            {
                List<Prenotazione> prenotazioni = await libreriaContext.Prenotazioni.Include(p => p.Libro).Where(p => p.UtenteId == utente.Id).ToListAsync();
                foreach (Prenotazione p in prenotazioni)
                {

                    Libro? libro = p.Libro;
                    if (libro == null)
                        return false;

                    libreriaContext.Prenotazioni.Remove(p);

                    if ((await Save()) > 0)
                    {
                        libro.Disponibilita++;
                        libreriaContext.Libri.Update(libro);

                        if (await Save() <= 0)
                            return false;
                    }
                }

                var Utente = await libreriaContext.Users.FirstOrDefaultAsync(u => u.Id == utente.Id);
                if (Utente == null)
                    return false;

                libreriaContext.Users.Remove(Utente);

                if (await Save() > 0)
                    return true;

                return false;
            }
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di eliminazione cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nell' eliminazione del utente: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio dell' eliminazione del ruolo: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nell' eliminazione del ruolo: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> Insert(Utente utente)
        {
            if (utente == null)
                return false;

            try
            {
                libreriaContext.Users.Add(utente);

                if (await Save() > 0)
                    return true;

                return false;
            }
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di inserimento cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nel inserimento dell' utente: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio del inserimento del ruolo: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nel inserimento del ruolo: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> Update(Utente utente)
        {
            if (utente == null)
                return false;

            try
            {
                libreriaContext.Users.Update(utente);

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
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nell' update: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio dell' update del utente: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nell' update del ruolo: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> Login(Utente utente)
        {
            try
            {
                await signInManager.SignInAsync(utente, false);
                return true;
            }
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di login cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nel login: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio del login del utente: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nel login del ruolo: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> Logout()
        {
            try
            {
                await signInManager.SignOutAsync();
                return true;
            }
            catch (OperationCanceledException opcanc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [OPER. CANCELLED EXCEPTION] Operazione di logout cancellata: {opcanc_ex.Message}");
            }
            catch (DbUpdateConcurrencyException updcc_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPD. CONCURRENCY EXCEPTION] Errore di concorrenza nel logout: {updcc_ex.Message}");
            }
            catch (DbUpdateException upd_ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [UPDATE EXCEPTION] Errore nel salvataggio del logout del utente: {upd_ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"[{DateTime.UtcNow:G}] [{ex.GetType().Name}] Errore nel logout del ruolo: {ex.Message}");
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
            if (success)
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
