using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;
using System.Text.Json;

namespace Proj_Biblioteca.Controllers;

public class PrenotazioniController : BaseController
{

    public PrenotazioniController(IHttpContextAccessor contextAccessor, ILogger<PrenotazioniController> logger, LibreriaContext DbContext) : base(contextAccessor, logger, DbContext)
    {
    }

    public async Task<IActionResult> Prenota(int idLibro)
    {
        UtenteViewModel? UtenteLoggato = await GetUser("/Prentazioni/Prenota");
        if (UtenteLoggato != null)
        {
            Libro? libro = await _libreria.Libri.AsNoTracking().FirstOrDefaultAsync(l => l.ID == idLibro);

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
        UtenteViewModel? UtenteLoggato = await UtenteViewModel.GetViewModel(_libreria,id);

        if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
        {

            IEnumerable<Prenotazione> prenotazioni = await _libreria.Prenotazioni.Include(p=> p.Libro).AsNoTracking().ToListAsync();

            foreach (var prenotazione in prenotazioni)
            {
                var Utente = await _libreria.Utenti.AsNoTracking().FirstOrDefaultAsync(u => u.ID == prenotazione.UtenteID);
                prenotazione.UtenteViewModel = new UtenteViewModel() { ID = Utente.ID, DDR = Utente.DDR, Email = Utente.Email, Nome = Utente.Nome, Ruolo = Utente.Ruolo };
            }

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
        UtenteViewModel? UtenteLoggato = await UtenteViewModel.GetViewModel(_libreria, id);

        if (UtenteLoggato != null)
        {

            IEnumerable<Prenotazione> prenotazioni = await _libreria.Prenotazioni.Include(p => p.Libro).AsNoTracking().Where(p => p.UtenteID == UtenteLoggato.ID).ToListAsync();

            foreach (var prenotazione in prenotazioni)
            {
                prenotazione.UtenteViewModel = new UtenteViewModel() { ID = UtenteLoggato.ID, DDR = UtenteLoggato.DDR, Email = UtenteLoggato.Email, Nome = UtenteLoggato.Nome, Ruolo = UtenteLoggato.Ruolo };
            }


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
        UtenteViewModel? UtenteLoggato = await GetUser("Prenotazioni/RimuoviPrenotazione");
        if (UtenteLoggato != null)
        {
            Prenotazione? prenotazione;

            if (UtenteLoggato.Ruolo == "Admin")
                prenotazione = await _libreria.Prenotazioni.Include(p => p.Libro).AsNoTracking().FirstOrDefaultAsync(p => p.ID == id);
            else
                prenotazione = await _libreria.Prenotazioni.Include(p => p.Libro).AsNoTracking().Where(p => p.UtenteID == UtenteLoggato.ID).FirstOrDefaultAsync(p => p.ID == id);

            try
            {



                if (prenotazione != null)
                {
                    _libreria.Remove(prenotazione);
                    await _libreria.SaveChangesAsync();

                    Libro? libro = prenotazione.Libro;

                    libro.Disponibilita++;
                    _libreria.Update(libro);
                    await _libreria.SaveChangesAsync();

                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Prenotazione rimossa alle ore {DateTime.Now:HH:mm:ss}");
                    return Ok("PrenotazioneRimossa");
                }
                else
                {
                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} rimozione Prenotazione fallita alle ore {DateTime.Now:HH:mm:ss}");
                }
            }
            catch (DbUpdateException ex)
            {
                //Errore database 
                _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                return StatusCode(500);
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
    public async Task<IActionResult> AggiungiPrenotazione(int? idLibro, string inizio, string fine)
    {
        if (idLibro == null || inizio == null || fine == null)
            return BadRequest("Inserisci tutti i dati");

        UtenteViewModel? UtenteLoggato = await GetUser("/Prenotazioni/AggiungiPrenotazione");
        DateTime dataInizio = DateTime.Parse(inizio) + DateTime.Now.TimeOfDay;
        DateTime dataFine = DateTime.Parse(fine) + DateTime.Now.TimeOfDay;

        Libro? libro = await _libreria.Libri.AsNoTracking().FirstOrDefaultAsync(l => l.ID == idLibro);

        IEnumerable<Prenotazione>? prenotazioniUtente;

        string apiUrl = "https://localhost:7139/Prenotazioni/GetPrenotazioni/" + UtenteLoggato.ID;
        using (var httpClient = new HttpClient())
        {
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            string prenotazioniCrypted = await response.Content.ReadAsStringAsync();
            string prenotazioniJson = Encryption.Decrypt(prenotazioniCrypted);


            if (response.IsSuccessStatusCode)
                prenotazioniUtente = JsonSerializer.Deserialize<IEnumerable<Prenotazione>>(prenotazioniJson);
            else
            {
                return BadRequest("Errore nel recupero delle prenotazioni riprovare..");
            }

        }

        if (libro == null)
            return RedirectToAction("Elenco", "Libro");

        if (UtenteLoggato != null)
        {
            if (prenotazioniUtente.Count() < 3 && //check se l'utente loggato ha meno di 3 prenotazioni
                prenotazioniUtente.Where(p => p.Libro.ID == libro.ID).Count() < 1) //Check se l'utente loggato ha già lo stesso libro
            {

                try
                {
                    if (DateTime.Parse(inizio).AddDays(1) >= DateTime.Now && DateTime.Parse(inizio) <= DateTime.Parse(fine) && libro.Disponibilita > 0 && DateTime.Parse(fine) <= DateTime.Parse(inizio).AddDays(libro.PrenotazioneMax + 1)) // check della data e della disponibilita
                    {
                        _libreria.Add(new Prenotazione() { LibroID = libro.ID, UtenteID = UtenteLoggato.ID, DDI = DateTime.Parse(inizio), DDF = DateTime.Parse(fine) });
                        await _libreria.SaveChangesAsync();


                        libro.Disponibilita--;
                        _libreria.Update(libro);
                        await _libreria.SaveChangesAsync();


                        _logger.LogInformation($"Utente: {UtenteLoggato.Nome} ID_Libro: {libro.ID} Prenotazione aggiunta alle ore {DateTime.Now:HH:mm:ss}");
                        return Ok();
                    }
                    else
                        return BadRequest();

                }
                catch (DbUpdateException ex)
                {
                    //Errore database 
                    _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                    return StatusCode(500);
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
    }
}
