using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proj_Biblioteca.Service;
using Proj_Biblioteca.ViewModels;


namespace Proj_Biblioteca.Controllers
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254", Justification = "<In sospeso>")]

    public class UtentiController(ILogger<BaseController> logger, ILibreriaManager libreriaManager) : BaseController(logger, libreriaManager)
    {
        public async Task<IActionResult> AccountPage()
        {
            UtenteViewModel? UtenteLoggato = await _libreriaManager.Utenti().GetLoggedUser(User);

            if (ViewData.ContainsKey("Messaggio"))
                ViewData["Messaggio"] = TempData["Messaggio"];
            else
                ViewData.Add("Messaggio", TempData["Messaggio"]);

            if (ViewData.ContainsKey("Utente"))
                ViewData["Utente"] = UtenteLoggato;
            else
                ViewData.Add("Utente", UtenteLoggato);

            if (ViewData.ContainsKey("IsRegistrazione"))
                ViewData["IsRegistrazione"] = TempData["IsRegistrazione"];
            else
                ViewData.Add("IsRegistrazione", TempData["IsRegistrazione"]);

            return View(await _libreriaManager.Utenti().PrenotazioniUtente(UtenteLoggato));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GestioneRuoli()
        {
            return await Task.Run(View);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiaRuolo(string id, string ruolo)
        {
            if (await _libreriaManager.Utenti().CambiaRuolo(id, ruolo))
                return Ok();

            return BadRequest();
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListaUtenti(string email)
        {
            List<UtenteViewModel> utenti = await _libreriaManager.Utenti().ListaUtenti(email);

            if (utenti.Count > 0)
                return Json(utenti);

            return BadRequest();
        }



        // ~/Utenti/Login/{email}{password}
        /*
         * controlla email e password e fa loggare un utente
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login([Bind]LoginViewModel loginView)
        {
            _logger.LogInformation($"Tentativo di accesso alle ore {DateTime.UtcNow:HH:mm:ss}");

            if (ModelState.IsValid)
            {
                TempData["Messaggio"] = await _libreriaManager.Utenti().Login(loginView);
            }
            else
            {
                TempData["Messaggio"] = "Errore nel login..";
            }

            TempData["IsRegistrazione"] = false;
            return RedirectToAction("AccountPage");
        }

        // ~/Utenti/Registrazione/{nome}{email}{password}
        /*
         * Controlla nome, email e password inseriti e crea un account
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Registrazione([Bind]RegistrazioneViewModel registrazioneView)
        {
            if (ModelState.IsValid)
                TempData["Messaggio"] = await _libreriaManager.Utenti().Registrazione(registrazioneView);
            else
                TempData["Messaggio"] = "Errore nella registrazione..";

            TempData["IsRegistrazione"] = true;
            return RedirectToAction("AccountPage");
        }

        // ~/Utenti/Disconnect
        /*
         * Disconnette l'utente loggato
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Disconnect()
        {
            _logger.LogInformation($"Utente disconnesso alle ore {DateTime.UtcNow:HH:mm:ss}");

            await _libreriaManager.Utenti().Disconnect();

            return RedirectToAction("AccountPage");
        }


        // ~/Utenti/Delete
        /*
         * Elimina l'account dell'utente loggato
         * e resetta tutte le prenotazioni
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete()
        {
            UtenteViewModel? UtenteLoggato = await _libreriaManager.Utenti().GetLoggedUser(User);

            if (UtenteLoggato != null && await _libreriaManager.Utenti().Delete(UtenteLoggato))
                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Eliminazione riuscita alle ore {DateTime.UtcNow:HH:mm:ss}");
            else
                _logger.LogInformation($"Errore, eliminazione fallita {DateTime.UtcNow:HH:mm:ss}");

            return RedirectToAction("AccountPage");
        }
    }
}

