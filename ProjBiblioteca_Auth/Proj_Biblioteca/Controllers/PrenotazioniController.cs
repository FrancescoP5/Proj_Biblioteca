﻿using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Proj_Biblioteca.Service;

namespace Proj_Biblioteca.Controllers;

public class PrenotazioniController(ILogger<BaseController> logger, ILibreriaManager libreriaManager) : BaseController(logger, libreriaManager)
{
    public async Task<IActionResult> Prenota(int idLibro)
    {
        UtenteViewModel? UtenteLoggato = await _libreriaManager.Utenti().GetLoggedUser(User);

        if (UtenteLoggato != null)
            return View(await _libreriaManager.Libri().FindLibro(idLibro));

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
        return Ok(await _libreriaManager.Prenotazioni().ElencoPrenotazioni());
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
        return Ok(await _libreriaManager.Prenotazioni().GetPrenotazioni(id));
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
        UtenteViewModel? UtenteLoggato = await _libreriaManager.Utenti().GetLoggedUser(User);
        if (UtenteLoggato != null)
        {
            if (await _libreriaManager.Prenotazioni().RimuoviPrenotazione(id, UtenteLoggato))
                return Ok();

            _logger.LogInformation($"Errore rimozione prenotazione {DateTime.Now:HH:mm:ss}");
        }
        else
            _logger.LogInformation($"Nessun Account loggato {DateTime.Now:HH:mm:ss}");

        return BadRequest();
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
        UtenteViewModel? UtenteLoggato = await _libreriaManager.Utenti().GetLoggedUser(User);

        if (UtenteLoggato == null)
            return BadRequest();

        CodiceStato stato = await _libreriaManager.Prenotazioni().AggiungiPrenotazione(idLibro, UtenteLoggato!.Id??"", inizio, fine);

        switch(stato)
        {
            case CodiceStato.Ok:
                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} ID_Libro: {idLibro} Prenotazione aggiunta alle ore {DateTime.Now:HH:mm:ss}");
                return Ok();

            case CodiceStato.Dati_Insufficienti:
                return BadRequest("Inserisci tutti i dati");

            case CodiceStato.Errore_Client:
                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Prenotazioni massime raggiunte {DateTime.Now:HH:mm:ss}");
                return BadRequest("Attenzione, Prenotazione massima raggiunta");

            case CodiceStato.Errore_Server:
                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} aggiunta Prenotazione fallita alle ore {DateTime.Now:HH:mm:ss}");
                return StatusCode(500, "Errore nel salvataggio dei cambiamenti");

            case CodiceStato.Errore:
                return BadRequest();

            default:
                return NotFound();
        }
    }
}

