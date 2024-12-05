using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Proj_Biblioteca.Controllers;

public class PrenotazioniController : BaseController
{
    public PrenotazioniController
        (
            ILogger<BaseController> logger,
            LibreriaContext Dbcontext,
            UserManager<Utente> userManager,
            SignInManager<Utente> signInManager,
            RoleManager<Role> roleManager
        ) : base(logger, Dbcontext, userManager, signInManager, roleManager) { }

    public async Task<IActionResult> Prenota(int idLibro)
    {
        UtenteViewModel? UtenteLoggato = await GetUser();
        if (UtenteLoggato != null)
        {
            Libro? libro = await repoLibri.GetLibro(idLibro);

            return View(libro);
        }
        return RedirectToAction("AccountPage", "Utenti");
    }

    // ~/Prenotazioni/ElencoPrenotazioni
    /*
     * Controlla se l'utente è Admin 
     * Ritorna tutte le prenotazioni di tutti gli utenti in forma IEnumerable<Prenotazioni>
     */
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ElencoPrenotazioni()
    {
        IEnumerable<Prenotazione?> prenotazioni = await repoPrenotazioni.GetPrenotazioni();

        string JsonPrenotazioni = prenotazioni.ToJson();

        return Ok(Encryption.Encrypt(JsonPrenotazioni));
    }

    // ~/Prenotazioni/GetPrenotazioni
    /*
     * Controlla se l'utente è Loggato
     * Ritorna tutte le prenotazioni dell'utente loggato in forma IEnumerable<Prenotazioni>
     */
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetPrenotazioni(string id)
    {
        IEnumerable<Prenotazione?> prenotazioni = await repoPrenotazioni.GetPrenotazioni(id);

        string JsonPrenotazioni = prenotazioni.ToJson();

        return Ok(Encryption.Encrypt(JsonPrenotazioni));


    }

    // ~/Prenotazioni/RimuoviPrenotazione/{id}
    /*
     * 
     * Controlla se l'utente Loggado
     * Se L'utente è Admin
     * Controlla se la prenotazione esiste
     * Se L'utente non è Admin
     * Controlla se la prenotazione esiste ed è dell'utente
     * 
     * Rimuove la prenotazione selezionata attraverso all'id
     */
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RimuoviPrenotazione(int id)
    {
        UtenteViewModel? UtenteLoggato = await GetUser();
        if (UtenteLoggato != null)
        {
            Prenotazione? prenotazione;

            prenotazione = await repoPrenotazioni.GetPrenotazione(id);

            if (UtenteLoggato.Ruolo == "Utente" && prenotazione != null)
                prenotazione = prenotazione.IdUtente == UtenteLoggato.Id ? prenotazione : null;

            if (prenotazione != null)
            {
                Libro? libro = prenotazione.Libro;
                if (libro == null)
                    return NotFound();

                bool DeleteResult = await repoPrenotazioni.Delete(prenotazione);

                if (DeleteResult)
                {
                    libro.Disponibilita++;
                    bool UpdateResult = await repoLibri.Update(libro);
                    if (UpdateResult)
                    {
                        _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Prenotazione rimossa alle ore {DateTime.Now:HH:mm:ss}");
                        return Ok("PrenotazioneRimossa");
                    }
                }
                else
                {
                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} rimozione Prenotazione fallita alle ore {DateTime.Now:HH:mm:ss}");
                    return StatusCode(500, "Errore nel salvataggio dei cambiamenti");
                }

            }

        }
        else
        {
            _logger.LogInformation($"Nessun Account loggato {DateTime.Now:HH:mm:ss}");
        }
        return BadRequest("Errore Rimozione Prenotazione");
    }


    // ~/Prenotazioni/AggiungiPrenotazioni/{idLibro}{inizio}{fine}
    /*
     * Controlla se l'utente è Loggato
     * Aggiunge una Prenotazione all'utente loggato
     * alla prenotazione viene assegnata il libro selezionato, la data del ritiro e del ritorno
     * 
     * Controlla se l'utente ha meno di 3 prenotazioni o non ha già prenotato lo stesso libro
     */
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AggiungiPrenotazione(int? idLibro, string inizio, string fine)
    {
        UtenteViewModel? UtenteLoggato = await GetUser();

        if (UtenteLoggato == null)
            return BadRequest();

        if (idLibro == null || inizio == null || fine == null)
            return BadRequest("Inserisci tutti i dati");

        Libro? libro = await repoLibri.GetLibro(idLibro ?? 0);

        IEnumerable<Prenotazione?> prenotazioniUtente = await repoPrenotazioni.GetPrenotazioni(UtenteLoggato.Id ?? "0");

        if (libro == null)
            return RedirectToAction("Elenco", "Libro");

        if (prenotazioniUtente == null)
            return BadRequest();

        if (prenotazioniUtente.Count() < 3 && //check se l'utente loggato ha meno di 3 prenotazioni
            !prenotazioniUtente.Any(p => p.LibroID == libro.ID)) //Check se l'utente loggato ha già lo stesso libro
        {


            bool AddResult = await repoPrenotazioni.Insert(new Prenotazione() { LibroID = libro.ID, IdUtente = UtenteLoggato.Id, DDI = DateTime.Parse(inizio), DDF = DateTime.Parse(fine) });

            if (AddResult)
            {
                libro.Disponibilita--;
                bool UpdateResult = await repoLibri.Update(libro);
                if (UpdateResult)
                {
                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} ID_Libro: {libro.ID} Prenotazione aggiunta alle ore {DateTime.Now:HH:mm:ss}");
                    return Ok();
                }
            }
        }
        else
        {
            _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Prenotazioni massime raggiunte {DateTime.Now:HH:mm:ss}");
            return BadRequest("Attenzione, Prenotazione massima raggiunta");
        }

        _logger.LogInformation($"Utente: {UtenteLoggato.Nome} aggiunta Prenotazione fallita alle ore {DateTime.Now:HH:mm:ss}");
        return StatusCode(500, "Errore nel salvataggio dei cambiamenti");

    }
}

