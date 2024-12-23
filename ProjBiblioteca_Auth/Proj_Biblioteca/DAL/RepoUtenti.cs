﻿using Microsoft.AspNetCore.Identity;
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

    public class RepoUtenti(LibreriaContext libreriaContext, SignInManager<Utente> signInManager, UserManager<Utente> userManager) : IRepoUtenti, IDisposable
    {

        private readonly LibreriaContext libreriaContext = libreriaContext;
        private readonly SignInManager<Utente> signInManager = signInManager;
        private readonly UserManager<Utente> userManager = userManager;

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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return false;
            }
            return true;
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        public async Task<bool> InsertByCredentials(string nome, string email, string password)
        {
            Utente utente = new() { Id = Guid.NewGuid().ToString(), UserName = nome, Email = email, PasswordHash = password, DDR = DateTime.UtcNow };
            utente.PasswordHash = Encryption.HashPassword(password, utente);

            IdentityResult registerResult = await userManager.CreateAsync(utente);

            if (registerResult.Succeeded)
            {
                await userManager.AddToRoleAsync(utente, "Utente");
                return true;
            }

            return false;
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

        public async Task<bool> Login(Utente utente)
        {
            try
            {
                await signInManager.SignInAsync(utente, false);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public async Task<bool> Logout()
        {
            try
            {
                await signInManager.SignOutAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
            if (success)
                this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


}
