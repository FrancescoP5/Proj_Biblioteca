using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using System.Collections;
using System.Net;
using System.Text;
using System.Text.Unicode;
namespace Proj_Biblioteca.Controllers;

public class PrenotazioniController : BaseController
{

    public PrenotazioniController(IHttpContextAccessor contextAccessor, ILogger<PrenotazioniController> logger) : base(contextAccessor, logger)
    {
    }

    [Route("{controller}/{action}/{idLibro?}")]
    public async Task<IActionResult> Prenota(int idLibro)
    {
        Utente? UtenteLoggato = await GetUser();
        if (UtenteLoggato != null)
        {
            Libro libro = new Libro();
            libro = (Libro)await DAOLibro.GetInstance().Find(idLibro);

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
    public async Task<IActionResult> ElencoPrenotazioni()
    {
        Utente? UtenteLoggato = await GetUser();
        if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
        {
            return Ok((await DAOUtente.GetInstance().ElencoPrenotazioni()).Cast<Prenotazione>());
        }
        return NotFound();
    }

    // ~/Prenotazioni/GetPrenotazioni
    /*
     * Controlla se l'utente è Loggato
     * Ritorna tutte le prenotazioni dell'utente loggato in forma IEnumerable<Prenotazioni>
     */
    [HttpGet]
    public async Task<IActionResult> GetPrenotazioni()
    {
        Utente? UtenteLoggato = await GetUser();
        if (UtenteLoggato != null)
        {
            return Ok((await DAOUtente.GetInstance().PrenotazioniUtente(UtenteLoggato)).Cast<Prenotazione>());
        }
        return NotFound();

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
    public async Task<IActionResult> RimuoviPrenotazione(int id)
    {
        Utente? UtenteLoggato = await GetUser();
        if (UtenteLoggato != null)
        {
            Prenotazione? prenotazione;
            if (UtenteLoggato.Ruolo == "Admin")
                prenotazione = (Prenotazione?)(await DAOUtente.GetInstance().FindPrenotazione(id));
            else
                prenotazione = (Prenotazione?)(await DAOUtente.GetInstance().PrenotazioniUtente(UtenteLoggato)).Find(p => p.Id == id);

            if (prenotazione != null && await DAOUtente.GetInstance().RimuoviPrenotazione(prenotazione) )
            {
                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Prenotazione rimossa alle ore {DateTime.Now:HH:mm:ss}");
                return Ok("PrenotazioneRimossa");
            }
            else
            {
                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} rimozione Prenotazione fallita alle ore {DateTime.Now:HH:mm:ss}");
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
    public async Task<IActionResult> AggiungiPrenotazione(int idLibro, string inizio, string fine)
    {
        Utente? UtenteLoggato = await GetUser();
        DateTime dataInizio = DateTime.Parse(inizio) + DateTime.Now.TimeOfDay;
        DateTime dataFine = DateTime.Parse(fine) + DateTime.Now.TimeOfDay; 

        Libro libro = (Libro)await DAOLibro.GetInstance().Find(idLibro);

        List<Prenotazione> prenotazioniUtente;

        string apiUrl = "https://localhost:7139/Prenotazioni/GetPrenotazioni";
        using (var httpClient = new HttpClient())
        { 
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
            if(response.IsSuccessStatusCode)
                prenotazioniUtente = (await response.Content.ReadAsAsync<IEnumerable<Prenotazione>>()).ToList();
            else
                return RedirectToAction("Prenota", new { idLibro = idLibro });

        }

        if (libro == null)
            return RedirectToAction("Elenco","Libro");

        if (UtenteLoggato != null)
        {
            if (prenotazioniUtente.Count < 3 && //check se l'utente loggato ha meno di 3 prenotazioni
                prenotazioniUtente.Find(p => p.Libro.Id == libro.Id) == null) //Check se l'utente loggato ha già lo stesso libro
            {
                if (await DAOUtente.GetInstance().AggiungiPrenotazione(UtenteLoggato, libro, dataInizio, dataFine))
                {
                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} ID_Libro: {libro.Id} Prenotazione aggiunta alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("AccountPage", "Utenti");

                }

                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} ID_Libro: {libro.Id} aggiunta Prenotazione fallita alle ore {DateTime.Now:HH:mm:ss}");
            }
        }
        else
        {
            _logger.LogInformation($"Nessun Account loggato {DateTime.Now:HH:mm:ss}");
        }

        return RedirectToAction("Prenota", new { idLibro = idLibro });
    }
}
