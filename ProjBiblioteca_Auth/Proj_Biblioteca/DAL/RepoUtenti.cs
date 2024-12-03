using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Proj_Biblioteca.DAL
{
    public class RepoUtenti : IRepoUtenti, IDisposable
    {

        private readonly LibreriaContext libreriaContext;

        public RepoUtenti(LibreriaContext libreriaContext)
        {
            this.libreriaContext = libreriaContext;
        }

        public async Task<IEnumerable<Utente?>> GetUtenti()
        {
            try
            {
                return await libreriaContext.Users.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Enumerable.Empty<Utente?>();
            }
        }

        public async Task<Utente?> GetUtente(string id)
        {
            try
            {
                return await libreriaContext.Users.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<IdentityUserRole<string>?> GetUserRole(string id)
        {
            try
            {
                return await libreriaContext.UserRoles.AsNoTracking().FirstOrDefaultAsync(l => l.UserId == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<Role?> GetRuolo(string id)
        {
            try
            {
                return await libreriaContext.Roles.AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<bool> CambiaRuolo(string id, string ruolo, UserManager<Utente> manager)
        {
            var oldRoleID = (await GetUserRole(id))?.RoleId;
            var oldRole = (await GetRuolo(oldRoleID ?? "0"))?.Name;
            Utente? utente = await libreriaContext.FindAsync<Utente>(id);
            if (oldRole != ruolo)
            {
                if(utente == null)
                    return false;

                try
                {
                    await manager.RemoveFromRoleAsync(utente, oldRole ?? "Utente");
                    await manager.AddToRoleAsync(utente, ruolo);
                    await manager.UpdateSecurityStampAsync(utente);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return false;
            }
            return true;
        }

        public async Task<Utente?> Login(string email, string passwordHash)
        {
            if (!MailAddress.TryCreate(email, out _))//check della validita email
                return null;
            
            try
            {
                return await libreriaContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<bool> Registrazione(string nome, string email, string password, UserManager<Utente> manager)
        {
            string passwordRGX = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,10}$"; //Regex per la validazione di una password
            if (MailAddress.TryCreate(email, out _) && Regex.Match(password, passwordRGX).Success)
            {
                Utente utente = new() { Id = Guid.NewGuid().ToString(), UserName = nome, Email = email, PasswordHash = Encryption.Encrypt(password), DDR = DateTime.Now };

                IdentityResult registerResult = await manager.CreateAsync(utente);

                if (registerResult.Succeeded)
                {
                    await manager.AddToRoleAsync(utente, "Utente");
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> Delete(Utente? utente)
        {
            if (utente == null)
                return false;

            try
            {
                List<Prenotazione> prenotazioni = await libreriaContext.Prenotazioni.Include(p => p.Libro).Where(p => p.IdUtente == utente.Id).ToListAsync();
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

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
            if (!this.disposed)
            {
                if (disposing)
                {
                    libreriaContext.DisposeAsync();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
