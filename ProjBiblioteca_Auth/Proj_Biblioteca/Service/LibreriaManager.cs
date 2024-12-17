using Newtonsoft.Json;
using NuGet.Protocol;
using Proj_Biblioteca.DAL;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Utils;
using Proj_Biblioteca.ViewModels;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Proj_Biblioteca.Service
{
    public enum CodiceStato
    {
        Nulla = -1,
        Ok,
        Errore,
        Errore_Client,
        Errore_Server,
        Dati_Incorretti,
        Dati_Insufficienti,
        Non_Autorizzato
    }

    /// <summary>
    /// Accesso ai servizi di gestione della libreria
    /// <br/> <see cref="IRepoUtenti"/>
    /// <br/> <see cref="IRepoLibri"/>
    /// <br/> <see cref="IRepoPrenotazioni"/>
    /// </summary>
    public interface ILibreriaManager
    {
        Utenti Utenti();
        Libri Libri();
        Prenotazioni Prenotazioni();
    }

    /// <summary>
    /// Classe di accesso a tutti i servizi di gestione della libreria
    /// <br/> <see cref="ILibreriaManager"/> <seealso cref="LibreriaManager"/>
    /// </summary>
    public class LibreriaManager(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti) : ILibreriaManager
    {
        protected readonly IRepoPrenotazioni _repoPrenotazioni = repoPrenotazioni;
        protected readonly IRepoLibri _repoLibri = repoLibri;
        protected readonly IRepoUtenti _repoUtenti = repoUtenti;

        public Utenti Utenti()
        {
            return this as Utenti ?? new(_repoPrenotazioni, _repoLibri, _repoUtenti);
        }
        public Libri Libri()
        {
            return this as Libri ?? new(_repoPrenotazioni, _repoLibri, _repoUtenti);
        }
        public Prenotazioni Prenotazioni()
        {
            return this as Prenotazioni ?? new(_repoPrenotazioni, _repoLibri, _repoUtenti);
        }
    }

    /// <summary>
    /// Classe di accesso ai servizi di gestione degli utenti 
    /// </summary>
    public class Utenti(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti) : LibreriaManager(repoPrenotazioni, repoLibri, repoUtenti)
    {
        public async Task<UtenteViewModel?> GetLoggedUser(ClaimsPrincipal User)
        {
            var id = User.Claims.FirstOrDefault()?.Value;

            if (id == null)
                return null;


            return await GetViewModel(id);
        }

        public async Task<UtenteViewModel?> GetViewModel(string id)
        {
            if (id == null)
                return null;

            var user = await _repoUtenti!.GetUtente(id);

            if (user == null)
                return null;
            var ruoloID = (await _repoUtenti.GetUserRole(id))?.RoleId;
            var ruolo = (await _repoUtenti.GetRuolo(ruoloID ?? ""))?.Name;

            return new UtenteViewModel()
            {
                Id = user.Id,
                DDR = user.DDR,
                Email = user.Email ?? "null",
                Nome = user.UserName ?? "null",
                Ruolo = ruolo ?? "Utente"
            };
        }

        public async Task<List<UtenteViewModel>> GetViewModels()
        {
            var users = await _repoUtenti!.GetUtenti();

            List<UtenteViewModel> utentiViewModel = [];
            foreach (var user in users)
            {
                if (users == null)
                    continue;

                var ruoloID = (await _repoUtenti.GetUserRole(user!.Id))?.RoleId;
                var ruolo = (await _repoUtenti.GetRuolo(ruoloID ?? ""))?.Name;

                utentiViewModel.Add(new UtenteViewModel()
                {
                    Id = user.Id,
                    DDR = user.DDR,
                    Email = user.Email ?? "null",
                    Nome = user.UserName ?? "null",
                    Ruolo = ruolo ?? "Utente"
                });
            }

            return utentiViewModel;
        }

        public async Task<Tuple<IEnumerable<Prenotazione?>,int>> PrenotazioniUtente(UtenteViewModel? utente, int? page, string? search, int? ordinaDDI, int? ordinaDDF)
        {


            if (utente == null)
                return new(Enumerable.Empty<Prenotazione>(),0);

            if (utente.Ruolo == "Admin")
            {
                var json = await Prenotazioni().ElencoPrenotazioni(page ?? 1, search, ordinaDDI, ordinaDDF);

                Tuple<IEnumerable<Prenotazione?>,int> prenotazioni = JsonConvert.DeserializeObject<Tuple<IEnumerable<Prenotazione?>, int>>(Encryption.Decrypt(json)) ?? new(Enumerable.Empty<Prenotazione>(),0);
                return prenotazioni;
            }

            return new(JsonConvert.DeserializeObject<IEnumerable<Prenotazione?>>(Encryption.Decrypt(await Prenotazioni().GetPrenotazioni(utente.Id ?? ""))) ?? Enumerable.Empty<Prenotazione>(),0);
        }

        public async Task<bool> CambiaRuolo(string id, string ruolo)
        {
            return await _repoUtenti.CambiaRuolo(id, ruolo);
        }

        public async Task<List<UtenteViewModel>> ListaUtenti(string email = "")
        {
            if (string.IsNullOrEmpty(email))
            {
                return await GetViewModels();
            }
            else
            {
                return (await GetViewModels()).Where(u => u.Email.Contains(email, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
        }

        public async Task<string> Login(LoginViewModel loginView)
        {
            string message = "Errore! Qualcosa è andato storto, riprovare..";

            if (!MailAddress.TryCreate(loginView.Email, out _))//check della validita email
                return message;

            Utente? utente = await _repoUtenti.GetByCredentials(loginView.Email, loginView.Password ?? "");

            if (utente != null)
            {
                if (await _repoUtenti!.Login(utente))
                    message = "Successo! Verrai reindirizzato a breve..";
                else
                    message = "Errore! Login fallito, riprovare..";
            }
            else
            {
                message = "Errore! Credenziali errate, riprovare..";
            }

            return message;

        }

        public async Task<string> Registrazione(RegistrazioneViewModel registrazioneView)
        {
            if (registrazioneView.Password != registrazioneView.ConfermaPassword)
                return "Attenzione, le password non corrispondono";

            string passwordRGX = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,200}$"; //Regex per la validazione di una password
            if (!Regex.Match(registrazioneView.Password ?? "", passwordRGX).Success)
                return "Errore password invalida";

            if (!MailAddress.TryCreate(registrazioneView.Email, out _))
                return "Errore email invalida";

            if (await _repoUtenti.InsertByCredentials(registrazioneView.Nome ?? "", registrazioneView.Email, registrazioneView.Password ?? "")) 
                return await Login(new LoginViewModel() { Email = registrazioneView.Email, Password = registrazioneView.Password});

            return "Errore! Registrazione fallita, riprovare..";
        }

        public async Task<bool> Disconnect()
        {
            return await _repoUtenti!.Logout();
        }

        public async Task<bool> Delete(UtenteViewModel utente)
        {
            if (utente == null) return false;

            var Utente = await _repoUtenti.GetUtente(utente.Id ?? "0");
            if (Utente == null) return false;

            return await _repoUtenti!.Delete(Utente);
        }
    }

    /// <summary>
    /// Classe di accesso ai servizi di gestione dei libri
    /// </summary>
    public class Libri(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti) : LibreriaManager(repoPrenotazioni, repoLibri, repoUtenti)
    {

        public async Task<Tuple<IEnumerable<Libro?>,int>> Elenco(int? page, string? search)
        {
            int pageSize = 9;

            page ??= 1;

            IEnumerable<Libro?> libri = Enumerable.Empty<Libro>();

            Expression<Func<Libro, bool>>? filter = null;

            if (!string.IsNullOrEmpty(search))
            {
                filter = l => (l.Titolo ?? "").ToLower().Contains(search.ToLower());

                libri = await _repoLibri.GetListAsync(filter, page, pageSize);
            }
            else
            {
                libri = await _repoLibri.GetListAsync(filter, page, pageSize);
            }

            int totalPages = await _repoLibri.PageCountAsync(pageSize, filter);

            return new(libri,totalPages);
            
        }


        public async Task<Libro?> FindLibro(int? id)
        {
            return await _repoLibri.GetAsync(l=>l.ID == (id??0));
        }

        public async Task<bool> Aggiungi(Libro libro)
        {
            if ((libro.Titolo ?? "").Length > 300 || (libro.Autore ?? "").Length > 300)
                return false;

            if (libro.Disponibilita < 0 || libro.PrenotazioneMax < 0)
                return false;

            return await _repoLibri.Insert(libro);
        }

        public async Task<bool> Aggiorna(Libro libro)
        {
            if ((libro.Titolo??"").Length > 300 || (libro.Autore ?? "").Length > 300)
                return false;

            if (libro.Disponibilita < 0 || libro.PrenotazioneMax < 0)
                return false;

            return await _repoLibri.Update(libro);
        }

        public async Task<bool> Elimina(int? id)
        {
            Libro? libro = await FindLibro(id);

            if (libro == null) return false;

            return await _repoLibri.Delete(libro);

        }
    }

    /// <summary>
    /// Classe di accesso ai servizi di gestione delle prenotazioni
    /// </summary>
    public class Prenotazioni(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti) : LibreriaManager(repoPrenotazioni, repoLibri, repoUtenti)
    {
        public async Task<string> ElencoPrenotazioni(int page, string? search, int? ordinaDDI, int? ordinaDDF)
        {
            int pageSize = 5;


            Expression<Func<Prenotazione, bool>> filtri = x=> true;

            if (!string.IsNullOrEmpty(search))
                filtri = x => (x.Utente!.Email??"").ToLower().Contains(search.ToLower());

            Expression<Func<Prenotazione,object>>? ordina = null;
            bool desc = false;


            if(ordinaDDI.HasValue && ordinaDDI>=0)
            {
                ordina = x => x.DDI;
                desc = Convert.ToBoolean(ordinaDDI);
            }
            else if(ordinaDDF.HasValue && ordinaDDF >= 0)
            {
                ordina = x => x.DDF;
                desc = Convert.ToBoolean(ordinaDDF);
            }

            Tuple<IEnumerable<Prenotazione?>,int> prenotazioni = new(Enumerable.Empty<Prenotazione?>(),0);
            int pageCount = await _repoPrenotazioni.PageCountAsync(pageSize, filtri);

            if (ordina != null)
                prenotazioni = new(await _repoPrenotazioni.GetListAsync(filtri, ordina, desc, page, pageSize), pageCount);
            else
                prenotazioni = new(await _repoPrenotazioni.GetListAsync(filtri, page, pageSize), pageCount);

            foreach (var prenotazione in prenotazioni.Item1)
            {
                if (prenotazione != null)
                    prenotazione.UtenteViewModel = await Utenti().GetViewModel(prenotazione.UtenteId ?? "");
            }
            string JsonPrenotazioni = prenotazioni.ToJson();

            return Encryption.Encrypt(JsonPrenotazioni);
        }

        public async Task<string> GetPrenotazioni(string id)
        {
            Expression<Func<Prenotazione,bool>> filtro = p=>p.UtenteId == id;

            IEnumerable<Prenotazione?> prenotazioni = await _repoPrenotazioni.GetListAsync(filtro,1,10);

            foreach (var prenotazione in prenotazioni)
            {
                if (prenotazione != null)
                    prenotazione.UtenteViewModel = await this.Utenti().GetViewModel(prenotazione.UtenteId ?? "");
                
            }
            
            string JsonPrenotazioni = prenotazioni.ToJson();

            return Encryption.Encrypt(JsonPrenotazioni);


        }

        public async Task<bool> RimuoviPrenotazione(int id, UtenteViewModel utente)
        {
            Prenotazione? prenotazione = await _repoPrenotazioni.GetAsync(p=> p.ID == id);

            if (utente.Ruolo == "Utente" && prenotazione != null)
                prenotazione = prenotazione.UtenteId == utente.Id ? prenotazione : null;

            if (prenotazione == null) return false;


            Libro? libro = prenotazione.Libro;

            if (libro == null) return false;

            if (await _repoPrenotazioni.Delete(prenotazione))
            {
                libro.Disponibilita++;

                return await _repoLibri.Update(libro);
            }

            return false;
        }

        public async Task<CodiceStato> AggiungiPrenotazione(AddPrenotazioneViewModel prenotazione)
        {
            if (prenotazione.IdLibro == null || prenotazione.Inizio == null || prenotazione.Fine == null)
                return CodiceStato.Dati_Insufficienti;

            Libro? libro = await _repoLibri.GetAsync(l=>l.ID==(prenotazione.IdLibro??0));

            if (libro == null)
                return CodiceStato.Errore;

            var json = Encryption.Decrypt(await GetPrenotazioni(prenotazione.IdUtente??""));
            IEnumerable<Prenotazione?> prenotazioniUtente = JsonConvert.DeserializeObject<IEnumerable<Prenotazione?>>(json) ?? Enumerable.Empty<Prenotazione>();

            if (prenotazioniUtente == null)
                return CodiceStato.Errore;

            if (prenotazioniUtente.Count() < 3 && //check se l'utente loggato ha meno di 3 prenotazioni
                !prenotazioniUtente.Any(p => p!.LibroID == libro.ID)) //Check se l'utente loggato ha già lo stesso libro
            {
                DateTime DDI = DateTime.Parse(prenotazione.Inizio);
                DateTime DDF = DateTime.Parse(prenotazione.Fine);

                DDI = DDI.AddHours(prenotazione.ClientTime + prenotazione.TimeOffset);
                DDF = DDF.AddHours(prenotazione.ClientTime + prenotazione.TimeOffset);

                bool AddResult = await _repoPrenotazioni.Insert(new Prenotazione() { LibroID = libro.ID, UtenteId = prenotazione.IdUtente, DDI = DDI, DDF = DDF });

                if (AddResult)
                {
                    libro.Disponibilita--;
                    bool UpdateResult = await _repoLibri.Update(libro);

                    if (UpdateResult)
                        return CodiceStato.Ok;
                }
            }
            else
            {
                return CodiceStato.Errore_Client;
            }

            return CodiceStato.Errore_Server;
        }
    }
}
