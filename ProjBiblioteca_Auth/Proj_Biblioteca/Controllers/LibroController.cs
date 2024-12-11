using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Service;
using Proj_Biblioteca.ViewModels;

namespace Proj_Biblioteca.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254", Justification = "<In sospeso>")]
    public class LibroController(ILogger<BaseController> logger, ILibreriaManager libreriaManager) : BaseController(logger, libreriaManager)
    {
        [AllowAnonymous]
        public async Task<IActionResult> Elenco(string? search)
        {
            UtenteViewModel? UtenteLoggato = await _libreriaManager.Utenti().GetLoggedUser(User);

            if (ViewData.ContainsKey("Utente"))
                ViewData["Utente"] = UtenteLoggato;
            else
                ViewData.Add("Utente", UtenteLoggato);

            if(search == null)
            {
                ObjectResult? result = await GetLibri() as ObjectResult;
                return View(result!.Value);
            }
            else
            {
                return View(await _libreriaManager.Libri().Cerca(search));
            }

        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Modifica(int id)
        {
            ObjectResult? result = await FindLibro(id) as ObjectResult;

            return View(result!.Value);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AggiungiLibro()
        {
            return await Task.Run(() => View());
        }

        // ~/Libro/GetLibri
        /*
         * Ritorna tutti i libri
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetLibri()
        {
            return Ok(await _libreriaManager.Libri().Elenco());
        }

        // ~/Libro/GetLibri/search
        /*
         * Ritorna tutti i libri
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetLibri(string search)
        {
            return Ok(await _libreriaManager.Libri().Elenco());
        }

        // ~/Libro/FindLibro/{id}
        /*
         * Ritorna un libro attraverso l'id
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FindLibro(int id)
        {
            Libro? libro = await _libreriaManager.Libri().FindLibro(id);
            if (libro == null)
                return NotFound();

            return Ok(libro);
        }

        // ~/Libro/Aggiungi/{Libro}
        /*
         * Controlla se l'utente è Admin 
         * Aggiunge un libro
         * I parametri del libro vengono passati attraverso un form
         */
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Aggiungi([Bind("Titolo,Autore,PrenotazioneMax,ISBN,Disponibilita")] Libro libro)
        {
            if (ModelState.IsValid && await _libreriaManager.Libri().Aggiungi(libro))
            {
                _logger.LogInformation($"Libro: {libro.Titolo} Aggiunto alle ore {DateTime.UtcNow:HH:mm:ss}");

                return RedirectToAction("Elenco", "Libro");
            }

            _logger.LogInformation($"Libro: {libro.Titolo} Aggiunta fallita dati invalidi alle ore {DateTime.UtcNow:HH:mm:ss}");

            return RedirectToAction("AggiungiLibro", "Libro");

        }


        // ~/Libro/Aggiorna/{Libro}
        /*
         * Controlla se l'utente è Admin 
         * Aggiorna un libro
         * I parametri del libro vengono passati attraverso un form
         */
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Aggiorna([Bind("ID,Titolo,Autore,PrenotazioneMax,ISBN,Disponibilita")] Libro libro)
        {
            if (ModelState.IsValid && await _libreriaManager.Libri().Aggiorna(libro))
            {
                _logger.LogInformation($"Libro: {libro.Titolo} Aggiornato alle ore {DateTime.UtcNow:HH:mm:ss}");

                return RedirectToAction("Elenco", "Libro");
            }

            _logger.LogInformation($"Libro: {libro.Titolo} Aggiornamento fallito alle ore {DateTime.UtcNow:HH:mm:ss}");

            return RedirectToAction("Modifica", "Libro");
        }


        // ~/Libro/Elimina/{id}
        /*
         * Controlla se l'utente è Admin 
         * Elimina un libro associato all'id
         */
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Elimina(int? id)
        {

            if (await _libreriaManager.Libri().Elimina(id))
            {
                _logger.LogInformation($"Libro ID: {id} Rimosso alle ore {DateTime.UtcNow:HH:mm:ss}");
                return RedirectToAction("Elenco", "Libro");
            }

            _logger.LogInformation($"Libro ID: {id} Rimozione fallita alle ore {DateTime.UtcNow:HH:mm:ss}");
            return RedirectToAction("Modifica", "Libro", new { id });
        }

    }
}

