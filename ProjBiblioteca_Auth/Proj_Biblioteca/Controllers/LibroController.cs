using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Proj_Biblioteca.Controllers
{
    public class LibroController : BaseController
    {

        public LibroController
            (
                ILogger<BaseController> logger,
                LibreriaContext Dbcontext,
                UserManager<Utente> userManager,
                SignInManager<Utente> signInManager,
                RoleManager<Role> roleManager
            ) : base(logger, Dbcontext, userManager, signInManager, roleManager) { }



        [AllowAnonymous]
        public async Task<IActionResult> Elenco()
        {
            UtenteViewModel? UtenteLoggato = await GetUser();
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Modifica(int id)
        {

            string apiUrl = "https://localhost:7139/Libro/FindLibro/" + id;
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
            return Ok(await repoLibri.GetLibri());
        }


        // ~/Libro/FindLibro/{id}
        /*
         * Ritorna un libro attraverso l'id
         */
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> FindLibro(int id)
        {
            return Ok(await repoLibri.GetLibro(id));
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
            if (ModelState.IsValid)
            {
                bool result = await repoLibri.Insert(libro);

                if (result)
                {
                    //Mess aggiunta riuscita
                    _logger.LogInformation($"Libro: {libro.Titolo} Aggiunto alle ore {DateTime.Now:HH:mm:ss}");

                    return RedirectToAction("Elenco", "Libro");
                }
            }
            //Mess modello non valido
            _logger.LogInformation($"Libro: {libro.Titolo} Aggiunta fallita dati invalidi alle ore {DateTime.Now:HH:mm:ss}");

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
            if (ModelState.IsValid)
            {
                bool result = await repoLibri.Update(libro);
                if (result)
                {
                    //Mess aggiunta riuscita
                    _logger.LogInformation($"Libro: {libro.Titolo} Aggiornato alle ore {DateTime.Now:HH:mm:ss}");

                    return RedirectToAction("Elenco", "Libro");
                }

            }
            //Mess modello non valido
            _logger.LogInformation($"Libro: {libro.Titolo} Aggiornamento fallito alle ore {DateTime.Now:HH:mm:ss}");

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
            if (id == null)
            {
                _logger.LogInformation($"ID necessario, Rimozione fallita alle ore {DateTime.Now:HH:mm:ss}");
                return RedirectToAction("Elenco", "Libro");
            }

            var libro = await repoLibri.GetLibro(id??0);

            if (libro != null)
            {
                bool result = await repoLibri.Delete(libro);
                if (result)
                {
                    //Mess rimozione riuscita
                    _logger.LogInformation($"Libro ID: {id} Rimosso alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("Elenco", "Libro");
                }
            }
            _logger.LogInformation($"Libro ID: {id} Rimozione fallita alle ore {DateTime.Now:HH:mm:ss}");
            return RedirectToAction("Modifica", "Libro", new { id = id });
        }

    }
}

