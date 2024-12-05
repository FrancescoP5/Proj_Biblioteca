using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;

namespace Proj_Biblioteca.DAL
{
    public class RepoPrenotazioni : IRepoPrenotazioni, IDisposable
    {

        private readonly LibreriaContext libreriaContext;

        public RepoPrenotazioni(LibreriaContext libreriaContext)
        {
            this.libreriaContext = libreriaContext;
        }

        public async Task<IEnumerable<Prenotazione?>> GetPrenotazioni()
        {
            try
            {
                IEnumerable<Prenotazione?> prenotazioni = await libreriaContext.Prenotazioni.Include(p => p.Libro).AsNoTracking().ToListAsync();
                foreach (var prenotazione in prenotazioni)
                {
                    if (prenotazione != null)
                    {
                        Utente? Utente = await libreriaContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == prenotazione.IdUtente);
                        if (Utente != null)
                        {
                            prenotazione.UtenteViewModel = await UtenteViewModel.GetViewModel(libreriaContext, Utente.Id);
                        }
                    }
                }

                return prenotazioni;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Enumerable.Empty<Prenotazione?>();
            }
        }        
        public async Task<IEnumerable<Prenotazione?>> GetPrenotazioni(string idUtente)
        {
            try
            {
                IEnumerable<Prenotazione?> prenotazioni = await libreriaContext.Prenotazioni.Include(p => p.Libro).AsNoTracking().Where(p => p.IdUtente == idUtente).ToListAsync();

                foreach (var prenotazione in prenotazioni)
                {
                    if (prenotazione != null)
                    {
                        Utente? Utente = await libreriaContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == prenotazione.IdUtente);
                        if (Utente != null)
                        {
                            prenotazione.UtenteViewModel = await UtenteViewModel.GetViewModel(libreriaContext, Utente.Id);
                        }
                    }
                }

                return prenotazioni;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Enumerable.Empty<Prenotazione?>();
            }
        }

        public async Task<Prenotazione?> GetPrenotazione(int id)
        {
            try
            {
                return await libreriaContext.Prenotazioni.Include(p => p.Libro).AsNoTracking().FirstOrDefaultAsync(l => l.ID == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

        }

        public async Task<bool> Insert(Prenotazione prenotazione)
        {
            if (prenotazione == null)
                return false;

            Libro? libro = await libreriaContext.Libri.AsNoTracking().FirstOrDefaultAsync(l=>l.ID == prenotazione.LibroID);

            if (libro == null)
                return false;

            if (
                prenotazione.DDI.AddDays(1) >= DateTime.Now && 
                prenotazione.DDI <= prenotazione.DDF && 
                libro.Disponibilita > 0 && 
                prenotazione.DDF <= prenotazione.DDI.AddDays(libro.PrenotazioneMax + 1)
                )
            {
                try
                {
                    libreriaContext.Prenotazioni.Add(prenotazione);

                    if (await Save() > 0)
                        return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
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
                prenotazione.DDI.AddDays(1) >= DateTime.Now &&
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
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
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
