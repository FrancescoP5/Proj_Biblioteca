using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace Proj_Biblioteca.Controllers
{

    public class UtentiController : BaseController
    {
        public UtentiController
            (
                ILogger<BaseController> logger, 
                LibreriaContext Dbcontext, 
                UserManager<Utente> userManager, 
                SignInManager<Utente> signInManager, 
                RoleManager<Role> roleManager
            ) 
            : base(logger, Dbcontext, userManager, signInManager, roleManager){}

        public async Task<IActionResult> AccountPage()
        {
            UtenteViewModel? UtenteLoggato = await GetUser();
            IEnumerable<Prenotazione?> prenotazioni;

            if (ViewData.ContainsKey("Messaggio"))
                ViewData["Messaggio"] = TempData["Messaggio"];
            else
                ViewData.Add("Messaggio", TempData["Messaggio"]);

            if (UtenteLoggato != null)
            {
                if (UtenteLoggato.Ruolo == "Admin")
                {
                    prenotazioni = await repoPrenotazioni.GetPrenotazioni();

                    if (ViewData.ContainsKey("Utente"))
                        ViewData["Utente"] = UtenteLoggato;
                    else
                        ViewData.Add("Utente", UtenteLoggato);

                    return View(prenotazioni);
                }
                else
                {
                    prenotazioni = await repoPrenotazioni.GetPrenotazioni(UtenteLoggato.Id??"0");

                    if (ViewData.ContainsKey("Utente"))
                        ViewData["Utente"] = UtenteLoggato;
                    else
                        ViewData.Add("Utente", UtenteLoggato);

                    return View(prenotazioni);

                }
            }
            return View();
        }

        [Authorize(Roles="Admin")]
        public async Task<IActionResult> GestioneRuoli()
        {
                return await Task.Run(View);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CambiaRuolo(string id, string ruolo)
        {
            if (await repoUtenti.CambiaRuolo(id, ruolo, _userManager))
            {
                return Ok();
            }
            return BadRequest();
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListaUtenti(string email)
        {
            List<UtenteViewModel> utenti;

            if (string.IsNullOrEmpty(email))
            {
                utenti = await UtenteViewModel.GetViewModel(_libreria);
            }
            else
            {
                utenti = (await UtenteViewModel.GetViewModel(_libreria)).Where(u => u.Email.ToLower().Contains(email.ToLower())).ToList();
            }

            if (utenti.Count > 0)
            {
                return Json(utenti);
            }
            else
            {
                return BadRequest();
            }

        }



        // ~/Utenti/Login/{email}{password}
        /*
         * controlla email e password e fa loggare un utente
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
            _logger.LogInformation($"Tentativo di accesso alle ore {DateTime.Now:HH:mm:ss}");

            Utente? utente = await repoUtenti.Login(email, Encryption.Encrypt(password));

            if (utente != null)
            {
                try
                {
                    await _signInManager.SignInAsync(utente, false);
                }
                catch (Exception ex)
                {
                    TempData["Messaggio"] = "Errore.. riprovare";
                    _logger.LogError(ex.ToString());
                }
                return RedirectToAction("AccountPage");
            }
            else
            {
                TempData["Messaggio"] = "Credenziali Errate, riprovare il Login";
                return RedirectToAction("AccountPage");
            }

        }

        // ~/Utenti/Registrazione/{nome}{email}{password}
        /*
         * Controlla nome, email e password inseriti e crea un account
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Registrazione(string nome, string email, string password)
        {
            if (await repoUtenti.Registrazione(nome, email, password, _userManager))
                return await Login(email, password);

            //Errore database 
            TempData["Messaggio"] = "Errore, Registrazione Fallita riprovare.";
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
            _logger.LogInformation($"Utente disconnesso alle ore {DateTime.Now:HH:mm:ss}");
            try
            {
                await _signInManager.SignOutAsync();
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.ToString());
            }
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
            UtenteViewModel? UtenteLoggato = await GetUser();
            if (UtenteLoggato != null)
            {
                var Utente = await repoUtenti.GetUtente(UtenteLoggato.Id ?? "0");
                try
                {
                    await _signInManager.SignOutAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                await repoUtenti.Delete(Utente);

                _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Eliminazione riuscita alle ore {DateTime.Now:HH:mm:ss}");
            }
            else
            {
                _logger.LogInformation($"Nessun Account loggato {DateTime.Now:HH:mm:ss}");
            }
            return RedirectToAction("AccountPage");
        }
    }
}

