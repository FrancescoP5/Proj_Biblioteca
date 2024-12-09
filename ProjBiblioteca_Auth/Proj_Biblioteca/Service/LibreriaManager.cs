﻿using Newtonsoft.Json;
using NuGet.Protocol;
using Proj_Biblioteca.DAL;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Utils;
using Proj_Biblioteca.ViewModels;
using System.Net.Mail;
using System.Security.Claims;

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
    public class LibreriaManager : ILibreriaManager
    {

        protected readonly IRepoPrenotazioni _repoPrenotazioni;
        protected readonly IRepoLibri _repoLibri;
        protected readonly IRepoUtenti _repoUtenti;

        public LibreriaManager(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti)
        {
            _repoPrenotazioni = repoPrenotazioni;
            _repoLibri = repoLibri;
            _repoUtenti = repoUtenti;
        }

        public Utenti Utenti()
        {
            return this as Utenti?? new(_repoPrenotazioni, _repoLibri, _repoUtenti);
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
    public class Utenti: LibreriaManager
    {
        public Utenti(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti) : base(repoPrenotazioni, repoLibri, repoUtenti)
        {
        }

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
            var ruolo = (await _repoUtenti.GetRuolo(ruoloID??""))?.Name;

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
                var ruolo = (await _repoUtenti.GetRuolo(ruoloID??""))?.Name;

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

        public async Task<IEnumerable<Prenotazione?>> PrenotazioniUtente(UtenteViewModel? utente)
        {
            if (utente == null)
                return Enumerable.Empty<Prenotazione>();

            if (utente.Ruolo == "Admin")
                return JsonConvert.DeserializeObject<IEnumerable<Prenotazione?>>(Encryption.Decrypt(await Prenotazioni().ElencoPrenotazioni())) ?? Enumerable.Empty<Prenotazione>();

            return JsonConvert.DeserializeObject<IEnumerable<Prenotazione?>>(Encryption.Decrypt(await Prenotazioni().GetPrenotazioni(utente.Id??""))) ?? Enumerable.Empty<Prenotazione>(); ;
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
                return (await GetViewModels()).Where(u => u.Email.ToLower().Contains(email.ToLower())).ToList();
            }
        }

        public async Task<string> Login(string email, string password)
        {
            string message = "Errore! Qualcosa è andato storto, riprovare..";

            if (!MailAddress.TryCreate(email, out _))//check della validita email
                return message;

            Utente? utente = await _repoUtenti.GetByCredentials(email, Encryption.Encrypt(password));

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

        public async Task<string> Registrazione(string nome, string email, string password)
        {
            if (await _repoUtenti.InsertByCredentials(nome, email, password))
                return await Login(email, password);

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
    public class Libri: LibreriaManager
    {
        public Libri(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti) : base(repoPrenotazioni, repoLibri, repoUtenti)
        {
        }

        public async Task<IEnumerable<Libro?>> Elenco()
        {
            return await _repoLibri.GetLibri();
        }

        public async Task<Libro?> FindLibro(int? id)
        {
            return await _repoLibri.GetLibro(id ?? 0);
        }

        public async Task<bool> Aggiungi(Libro libro)
        {
            return await _repoLibri.Insert(libro);
        }

        public async Task<bool> Aggiorna(Libro libro)
        {
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
    public class Prenotazioni : LibreriaManager
    {
        public Prenotazioni(IRepoPrenotazioni repoPrenotazioni, IRepoLibri repoLibri, IRepoUtenti repoUtenti) : base(repoPrenotazioni, repoLibri, repoUtenti)
        {
        }

        public async Task<string> ElencoPrenotazioni()
        {
            IEnumerable<Prenotazione?> prenotazioni = await _repoPrenotazioni.GetPrenotazioni();

            foreach (var prenotazione in prenotazioni)
            {
                if (prenotazione != null)
                    prenotazione.UtenteViewModel = await Utenti().GetViewModel(prenotazione.IdUtente ?? "");
            }

            string JsonPrenotazioni = prenotazioni.ToJson();

            return Encryption.Encrypt(JsonPrenotazioni);
        }

        public async Task<string> GetPrenotazioni(string id)
        {
            IEnumerable<Prenotazione?> prenotazioni = await _repoPrenotazioni.GetPrenotazioni(id);

            foreach (var prenotazione in prenotazioni)
            {
                if (prenotazione != null)
                    prenotazione.UtenteViewModel = await this.Utenti().GetViewModel(prenotazione.IdUtente ?? "");
            }

            string JsonPrenotazioni = prenotazioni.ToJson();

            return Encryption.Encrypt(JsonPrenotazioni);


        }

        public async Task<bool> RimuoviPrenotazione(int id, UtenteViewModel utente)
        {
            Prenotazione? prenotazione = await _repoPrenotazioni.GetPrenotazione(id);

            if (utente.Ruolo != "Utente" && prenotazione != null)
                prenotazione = prenotazione.IdUtente == utente.Id ? prenotazione : null;

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

        public async Task<CodiceStato> AggiungiPrenotazione(int? idLibro, string idUtente, string inizio, string fine)
        {
            if (idLibro == null || inizio == null || fine == null)
                return CodiceStato.Dati_Insufficienti;

            Libro? libro = await _repoLibri.GetLibro(idLibro ?? 0);

            IEnumerable<Prenotazione?> prenotazioniUtente = JsonConvert.DeserializeObject<IEnumerable<Prenotazione?>>(Encryption.Decrypt(await GetPrenotazioni(idUtente))) ?? Enumerable.Empty<Prenotazione>();



            if (libro == null)
                return CodiceStato.Errore;

            if (prenotazioniUtente == null)
                return CodiceStato.Errore;

            if (prenotazioniUtente.Count() < 3 && //check se l'utente loggato ha meno di 3 prenotazioni
                !prenotazioniUtente.Any(p => p!.LibroID == libro.ID)) //Check se l'utente loggato ha già lo stesso libro
            {
                bool AddResult = await _repoPrenotazioni.Insert(new Prenotazione() { LibroID = libro.ID, IdUtente = idUtente, DDI = DateTime.Parse(inizio), DDF = DateTime.Parse(fine) });

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
