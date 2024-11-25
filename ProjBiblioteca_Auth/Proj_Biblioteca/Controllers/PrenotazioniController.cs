using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;

namespace Proj_Biblioteca.Controllers;

public class PrenotazioniController : BaseController
{

    public PrenotazioniController(IHttpContextAccessor contextAccessor, ILogger<PrenotazioniController> logger) : base(contextAccessor, logger)
    {
    }

    public async Task<IActionResult> Prenota(int idLibro)
    {
        Utente? UtenteLoggato = await GetUser("/Prentazioni/Prenota");
        if (UtenteLoggato != null)
        {
            Libro libro = new Libro();
            //libro = (Libro)await DAOLibro.GetInstance().Find(idLibro);

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
    public async Task<IActionResult> ElencoPrenotazioni(int id)
    {
        //Utente? UtenteLoggato = (Utente)await DAOUtente.GetInstance().Find(id);
        Utente? UtenteLoggato = new Utente();

        if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
        {

            //IEnumerable<Prenotazione> prenotazioni = (await DAOUtente.GetInstance().ElencoPrenotazioni()).Cast<Prenotazione>();
            IEnumerable<Prenotazione> prenotazioni = null;

            string JsonPrenotazioni = prenotazioni.ToJson();

            return Ok(Encryption.Encrypt(JsonPrenotazioni));
        }
        return NotFound();
    }

    // ~/Prenotazioni/GetPrenotazioni
    /*
     * Controlla se l'utente è Loggato
     * Ritorna tutte le prenotazioni dell'utente loggato in forma IEnumerable<Prenotazioni>
     */
    [HttpGet]
    public async Task<IActionResult> GetPrenotazioni(int id)
    {
        //Utente? UtenteLoggato = (Utente)await DAOUtente.GetInstance().Find(id);
        Utente? UtenteLoggato = new Utente();

        if (UtenteLoggato != null)
        {

            //IEnumerable<Prenotazione> prenotazioni = (await DAOUtente.GetInstance().PrenotazioniUtente(UtenteLoggato)).Cast<Prenotazione>();
            IEnumerable<Prenotazione> prenotazioni = null;

            string JsonPrenotazioni = prenotazioni.ToJson();

            return Ok(Encryption.Encrypt(JsonPrenotazioni));
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
        Utente? UtenteLoggato = await GetUser("Prenotazioni/RimuoviPrenotazione");
        if (UtenteLoggato != null)
        {
            Prenotazione? prenotazione;
            if (UtenteLoggato.Ruolo == "Admin")
                //prenotazione = (Prenotazione?)(await DAOUtente.GetInstance().FindPrenotazione(id));
                prenotazione = null;
            else
                //prenotazione = (Prenotazione?)(await DAOUtente.GetInstance().PrenotazioniUtente(UtenteLoggato)).Find(p => p.Id == id);
                prenotazione = null;

            if (prenotazione != null /*&& await DAOUtente.GetInstance().RimuoviPrenotazione(prenotazione)*/ )
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
        if (idLibro == null || inizio == null || fine == null)
            return BadRequest("Inserisci tutti i dati");

        Utente? UtenteLoggato = await GetUser("/Prenotazioni/AggiungiPrenotazione");
        DateTime dataInizio = DateTime.Parse(inizio) + DateTime.Now.TimeOfDay;
        DateTime dataFine = DateTime.Parse(fine) + DateTime.Now.TimeOfDay;

        Libro libro = /*(Libro)await DAOLibro.GetInstance().Find(idLibro);*/ null;

        List<Prenotazione> prenotazioniUtente;

        string apiUrl = "https://localhost:7139/Prenotazioni/GetPrenotazioni/"+UtenteLoggato.ID;
        using (var httpClient = new HttpClient())
        { 
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            string prenotazioniCrypted = await response.Content.ReadAsStringAsync();
            string prenotazioniJson = Encryption.Decrypt(prenotazioniCrypted);


            if (response.IsSuccessStatusCode)
                prenotazioniUtente = (JsonSerializer.Deserialize<IEnumerable<Prenotazione>>(prenotazioniJson)).ToList();
            else
            {
                return BadRequest("Errore nel recupero delle prenotazioni riprovare..");
            }

        }

        if (libro == null)
            return RedirectToAction("Elenco","Libro");

        if (UtenteLoggato != null)
        {
            if (prenotazioniUtente.Count < 3 && //check se l'utente loggato ha meno di 3 prenotazioni
                prenotazioniUtente.Find(p => p.Libro.ID == libro.ID) == null) //Check se l'utente loggato ha già lo stesso libro
            {
                if (true/*await DAOUtente.GetInstance().AggiungiPrenotazione(UtenteLoggato, libro, dataInizio, dataFine)*/)
                {
                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} ID_Libro: {libro.ID} Prenotazione aggiunta alle ore {DateTime.Now:HH:mm:ss}");
                    return Ok();

                }
            }
            else
            {
                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Prenotazioni massime raggiunte {DateTime.Now:HH:mm:ss}");
                return BadRequest("Attenzione, Prenotazione massima raggiunta");
            }
        }
        else
        {
            _logger.LogInformation($"Nessun Account loggato {DateTime.Now:HH:mm:ss}");
            return BadRequest("Errore, Nessun account loggato");
        }
        _logger.LogInformation($"Utente: {UtenteLoggato.Nome} ID_Libro: {libro.ID} aggiunta Prenotazione fallita alle ore {DateTime.Now:HH:mm:ss}");
        return BadRequest("Errore");
    }
}
