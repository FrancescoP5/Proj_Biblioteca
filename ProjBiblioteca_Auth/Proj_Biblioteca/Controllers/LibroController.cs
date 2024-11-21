using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Differencing;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.Controllers
{
    public class LibroController : BaseController
    {


        public LibroController(IHttpContextAccessor contextAccessor, ILogger<LibroController> logger) : base(contextAccessor, logger)
        {

        }


        public async Task<IActionResult> Elenco()
        {
            Utente? UtenteLoggato = await GetUser();
            string apiUrl = "https://localhost:7139/Libro/GetLibri";

            IEnumerable<Libro> libri;

            using (var httpClient = new HttpClient())
            {

                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                libri = await response.Content.ReadAsAsync<IEnumerable<Libro>>();
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
            Utente? UtenteLoggato = await GetUser();
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
                return Ok((await DAOLibro.GetInstance().ReadAll()).Cast<Libro>());
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
                return Ok((Libro)await DAOLibro.GetInstance().Find(id));
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
        public async Task<IActionResult> Aggiungi(Libro Libro)
        {
            Utente? UtenteLoggato = await GetUser();
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                if (await DAOLibro.GetInstance().Insert(Libro))
                {
                    //Mess aggiunta riuscita
                    _logger.LogInformation($"Libro: {Libro.Titolo} Aggiunto alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("Elenco", "Libro");
                }
                else
                {
                    //Mess aggiunta fallita
                    _logger.LogInformation($"Libro: {Libro.Titolo} Aggiunta fallita alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("AggiungiLibro", "Libro");
                }
            }
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
        public async Task<IActionResult> Aggiorna(Libro Libro)
        {
            Utente? UtenteLoggato = await GetUser();
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                if (await DAOLibro.GetInstance().Update(Libro))
                {
                    //Mess update riuscito
                    _logger.LogInformation($"Libro: {Libro.Titolo} Aggiornato alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("Elenco", "Libro");
                }
                else
                {
                    //Mess update fallito
                    _logger.LogInformation($"Libro: {Libro.Titolo} Aggiornamento fallito alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("Modifica", "Libro", new { id = Libro.Id });
                }
            }
            return RedirectToAction("Elenco", "Libro");
        }


        // ~/Libro/Elimina/{id}
        /*
         * Controlla se l'utente è Admin 
         * Elimina un libro associato all'id
         */
        [HttpPost]
        public async Task<IActionResult> Elimina(int id)
        {
            Utente? UtenteLoggato = await GetUser();
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                if (await DAOLibro.GetInstance().Delete(id))
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
            return RedirectToAction("Modifica", "Libro", new { id = id });
        }
    }
}
