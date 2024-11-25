using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;

namespace Proj_Biblioteca.Controllers
{
    public class LibroController : BaseController
    {


        public LibroController(IHttpContextAccessor contextAccessor, ILogger<LibroController> logger, LibreriaContext DbContext) : base(contextAccessor, logger, DbContext)
        {

        }


        public async Task<IActionResult> Elenco()
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Libro/Elenco");
            string apiUrl = "https://localhost:7139/Libro/GetLibri";

            List<Libro> libri;

            using (var httpClient = new HttpClient())
            {

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                libri = await response.Content.ReadAsAsync<List<Libro>>();
                if (response.IsSuccessStatusCode)
                {
                    if (ViewData.ContainsKey("Utente"))
                        ViewData["Utente"] = UtenteLoggato;
                    else
                        ViewData.Add("Utente", UtenteLoggato);
                    return View(libri);
                }
            }
            return View();
        }

        public async Task<IActionResult> Modifica(int id)
        {

            string apiUrl = "https://localhost:7139/Libro/FindLibro/"+id;
            Libro libro;

            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                libro = await response.Content.ReadAsAsync<Libro>();
                if (response.IsSuccessStatusCode)
                {
                    return View(libro);
                }
            }

            return View(libro);
        }

        public async Task<IActionResult> AggiungiLibro()
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Libro/AggiungiLibro");
            if (UtenteLoggato!= null && UtenteLoggato.Ruolo == "Admin")
                return await Task.Run(() => View());

            else
                return Unauthorized();
        }




        // ~/Libro/GetLibri
        /*
         * Ritorna tutti i libri
         */
        [HttpGet]
        public async Task<IActionResult> GetLibri()
        {
            try
            {
                return Ok(await _libreria.Libri.AsNoTracking().ToListAsync());
            }
            catch
            {
                return NotFound();
            }
        }


        // ~/Libro/FindLibro/{id}
        /*
         * Ritorna un libro attraverso l'id
         */

        [HttpGet]
        public async Task<IActionResult> FindLibro(int id)
        {
            try
            {
                return Ok(await _libreria.Libri.AsNoTracking().FirstOrDefaultAsync(l => l.ID == id));
            }
            catch
            {
                return NotFound();
            }
        }

        // ~/Libro/Aggiungi/{Libro}
        /*
         * Controlla se l'utente è Admin 
         * Aggiunge un libro
         * I parametri del libro vengono passati attraverso un form
         */
        [HttpPost]
        public async Task<IActionResult> Aggiungi([Bind("Titolo,Autore,PrenotazioneMax,ISBN,Disponibilita")] Libro libro)
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Libro/Aggiungi");
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        _libreria.Add(libro);
                        if (await _libreria.SaveChangesAsync() > 0)
                        {
                            //Mess aggiunta riuscita
                            _logger.LogInformation($"Libro: {libro.Titolo} Aggiunto alle ore {DateTime.Now:HH:mm:ss}");

                            return RedirectToAction("Elenco", "Libro");
                        }
                        else
                        {
                            //Mess aggiunta fallita
                            _logger.LogInformation($"Libro: {libro.Titolo} Aggiunta fallita alle ore {DateTime.Now:HH:mm:ss}");

                            return RedirectToAction("AggiungiLibro", "Libro");
                        }

                    }
                    //Mess modello non valido
                    _logger.LogInformation($"Libro: {libro.Titolo} Aggiunta fallita dati invalidi alle ore {DateTime.Now:HH:mm:ss}");

                    return RedirectToAction("AggiungiLibro", "Libro");
                }
                catch (DbUpdateException ex)
                {
                    //Errore database 
                    _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                    ModelState.TryAddModelError($"{DateTime.Now:HH:mm:ss.ff}","ERRORE: Impossibile salvare i cambiamenti.");
                    return RedirectToAction("Elenco", "Libro");
                }

            }
            //Messaggio autorizzazione insufficiente
            _logger.LogInformation($"Permessi non Concessi per aggiunta libro {DateTime.Now:HH:mm:ss}");
            return RedirectToAction("Elenco", "Libro");
        }




        // ~/Libro/Aggiorna/{Libro}
        /*
         * Controlla se l'utente è Admin 
         * Aggiorna un libro
         * I parametri del libro vengono passati attraverso un form
         */
        [HttpPost]
        public async Task<IActionResult> Aggiorna([Bind("ID,Titolo,Autore,PrenotazioneMax,ISBN,Disponibilita")] Libro libro)
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Libro/Aggiorna");
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        _libreria.Update(libro);
                        if (await _libreria.SaveChangesAsync() > 0)
                        {
                            //Mess aggiunta riuscita
                            _logger.LogInformation($"Libro: {libro.Titolo} Aggiornato alle ore {DateTime.Now:HH:mm:ss}");

                            return RedirectToAction("Elenco", "Libro");
                        }
                        else
                        {
                            //Mess aggiunta fallita
                            _logger.LogInformation($"Libro: {libro.Titolo} Aggiornamento fallito alle ore {DateTime.Now:HH:mm:ss}");

                            return RedirectToAction("Modifica", "Libro");
                        }

                    }
                    //Mess modello non valido
                    _logger.LogInformation($"Libro: {libro.Titolo} Aggiornamento fallito, dati invalidi alle ore {DateTime.Now:HH:mm:ss}");

                    return RedirectToAction("Modifica", "Libro");
                }
                catch (DbUpdateException ex)
                {
                    //Errore database 
                    _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                    ModelState.TryAddModelError($"{DateTime.Now:HH:mm:ss.ff}", "ERRORE: Impossibile salvare i cambiamenti.");
                    return RedirectToAction("Elenco", "Libro");
                }
            }
            //Messaggio autorizzazione insufficiente
            _logger.LogInformation($"Permessi non Concessi per modifica libro {DateTime.Now:HH:mm:ss}");
            return RedirectToAction("Elenco", "Libro");
        }


        // ~/Libro/Elimina/{id}
        /*
         * Controlla se l'utente è Admin 
         * Elimina un libro associato all'id
         */
        [HttpPost]
        public async Task<IActionResult> Elimina(int? id)
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Libro/Elimina");
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                if(id == null)
                {
                    _logger.LogInformation($"ID necessario, Rimozione fallita alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("Elenco", "Libro");
                }

                var libro = await _libreria.Libri.FindAsync(id);

                if(libro == null)
                {
                    _logger.LogInformation($"Libro non trovato, Rimozione fallita alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("Elenco", "Libro");
                }

                try
                {
                    _libreria.Libri.Remove(libro);
                    if(await _libreria.SaveChangesAsync() > 0)
                    {
                        //Mess rimozione riuscita
                        _logger.LogInformation($"Libro ID: {id} Rimosso alle ore {DateTime.Now:HH:mm:ss}");
                        return RedirectToAction("Elenco", "Libro");
                    }
                    else
                    {
                        //Mess rimozione fallita
                        _logger.LogInformation($"Libro ID: {id} Rimozione fallita alle ore {DateTime.Now:HH:mm:ss}");
                        return RedirectToAction("Modifica", "Libro", new { id = id });
                    }
                }
                catch (DbUpdateException ex)
                {
                    //Errore database 
                    _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                    ModelState.TryAddModelError($"{DateTime.Now:HH:mm:ss.ff}", "ERRORE: Impossibile salvare i cambiamenti.");
                    return RedirectToAction("Elenco", "Libro");
                }
            }
            //Messaggio autorizzazione insufficiente
            _logger.LogInformation($"Permessi non Concessi per modifica libro {DateTime.Now:HH:mm:ss}");
            return RedirectToAction("Elenco", "Libro");
        }
    }
}
