using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Security.Policy;

namespace Proj_Biblioteca.Controllers
{

    public class UtentiController : BaseController
    {
        public UtentiController(IHttpContextAccessor contextAccessor, ILogger<BaseController> logger, LibreriaContext Dbcontext, UserManager<Utente> userManager, SignInManager<Utente> signInManager, RoleManager<Role> roleManager) : base(contextAccessor, logger, Dbcontext, userManager, signInManager, roleManager)
        {
        }

        public async Task<IActionResult> AccountPage()
        {
            UtenteViewModel? UtenteLoggato = await GetUser();
            IEnumerable<Prenotazione>? prenotazioni = null;



            if (ViewData.ContainsKey("Messaggio"))
                ViewData["Messaggio"] = TempData["Messaggio"];
            else
                ViewData.Add("Messaggio", TempData["Messaggio"]);

            if (UtenteLoggato != null)
            {
                if (UtenteLoggato.Ruolo == "Admin")
                {
                    string apiUrl = "https://localhost:7139/Prenotazioni/ElencoPrenotazioni/" + UtenteLoggato.Id;


                    using (var httpClient = new HttpClient())
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);


                        if (response.IsSuccessStatusCode)
                        {
                            string prenotazioniCrypted = await response.Content.ReadAsStringAsync();
                            string prenotazioniJson = Encryption.Decrypt(prenotazioniCrypted);
                            prenotazioni = JsonSerializer.Deserialize<IEnumerable<Prenotazione>>(prenotazioniJson);

                            if (ViewData.ContainsKey("Utente"))
                                ViewData["Utente"] = UtenteLoggato;
                            else
                                ViewData.Add("Utente", UtenteLoggato);

                            return View(prenotazioni);
                        }

                    }
                }
                else
                {
                    string apiUrl = "https://localhost:7139/Prenotazioni/GetPrenotazioni/" + UtenteLoggato.Id;


                    using (var httpClient = new HttpClient())
                    {

                        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);


                        if (response.IsSuccessStatusCode)
                        {

                            string prenotazioniCrypted = await response.Content.ReadAsStringAsync();
                            string prenotazioniJson = Encryption.Decrypt(prenotazioniCrypted);
                            prenotazioni = JsonSerializer.Deserialize<IEnumerable<Prenotazione>>(prenotazioniJson);

                            if (ViewData.ContainsKey("Utente"))
                                ViewData["Utente"] = UtenteLoggato;
                            else
                                ViewData.Add("Utente", UtenteLoggato);

                            return View(prenotazioni);
                        }
                    }
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
            UtenteViewModel? UtenteLoggato = await GetUser();


            try
            {
                Utente? utente = await _libreria.Users.FirstOrDefaultAsync(p => p.Id == id);
                if (utente != null)
                {
                    var oldRoleID = (await _libreria.UserRoles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == id))?.RoleId;
                    var oldRole = (await _libreria.Roles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == oldRoleID))?.Name;

                    if (oldRole != ruolo)
                    {
                        await _userManager.RemoveFromRoleAsync(utente, oldRole ?? "Utente");
                        await _userManager.AddToRoleAsync(utente, ruolo);
                        await _userManager.UpdateSecurityStampAsync(utente);
                    }
                    return Ok();
                }
                return BadRequest();
            }
            catch (DbUpdateException ex)
            {
                //Errore database 
                _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                return StatusCode(500);
            }

        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListaUtenti(string email)
        {
            UtenteViewModel? UtenteLoggato = await GetUser();
            if(UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
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
            return Unauthorized();
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




            if (MailAddress.TryCreate(email, out _))//check della validita email
            {
                Utente? utente = await _libreria.Users.AsNoTracking().FirstOrDefaultAsync(p => p.Email == email && p.PasswordHash == Encryption.Encrypt(password));

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
            TempData["Messaggio"] = "Errore nel Login riprovare..";
            return RedirectToAction("AccountPage");
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

            string passwordRGX = "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,10}$"; //Regex per la validazione di una password
            //fra gli 8-10 caratteri almeno una maiuscola una minuscola un numero e un carattere speciale (@$!%*?&)

            if (MailAddress.TryCreate(email, out _) == false || Regex.Match(password, passwordRGX).Success == false)
            {
                _logger.LogInformation($"Registrazione fallita alle ore {DateTime.Now:HH:mm:ss}");
                TempData["Messaggio"] = "Password o Email invalide per la Registazione";
                return RedirectToAction("AccountPage");
            }
            try
            {

                Utente utente = new() {Id = Guid.NewGuid().ToString() ,UserName = nome,Email = email, PasswordHash = Encryption.Encrypt(password), DDR=DateTime.Now};

                IdentityResult registerResult = await _userManager.CreateAsync(utente);

                if (registerResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(utente, "Utente");
                    //Messaggio di riuscita Registrazione
                    _logger.LogInformation($"Registrazione riuscita alle ore {DateTime.Now:HH:mm:ss}");
                }
               


                return await Login(email, password);
            }
            catch (Exception ex)
            {
                //Errore database 
                TempData["Messaggio"] = "Errore, Registrazione Fallita riprovare.";
                _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                return RedirectToAction("AccountPage");
            }


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
            await _signInManager.SignOutAsync();
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
                List<Prenotazione> prenotazioni;
                prenotazioni = await _libreria.Prenotazioni.Include(p => p.Libro).AsNoTracking().Where(p => p.IdUtente == UtenteLoggato.Id).ToListAsync();

                foreach (Prenotazione p in prenotazioni)
                {
                    try
                    {
                        Libro? libro = p.Libro;
                        if (libro == null)
                            return NotFound();

                        _libreria.Remove(p);
                        await _libreria.SaveChangesAsync();

                        libro.Disponibilita++;
                        _libreria.Update(libro);
                        await _libreria.SaveChangesAsync();

                    }
                    catch (Exception ex)
                    {
                        //Errore database 
                        _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                        return (RedirectToAction("AccountPage"));
                    }

                }
                try
                {
                    var Utente = await _libreria.Users.FirstOrDefaultAsync(u => u.Id == UtenteLoggato.Id);
                    if (Utente == null)
                    {
                        _logger.LogInformation($"Utente Id-{UtenteLoggato.Id} non trovato, eliminazione non riuscita {DateTime.Now:HH:mm:ss}");
                        return RedirectToAction("AccountPage");
                    }

                    await _signInManager.SignOutAsync();
                    _libreria.Remove(Utente);
                    await _libreria.SaveChangesAsync();

                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Eliminazione riuscita alle ore {DateTime.Now:HH:mm:ss}");


                    return RedirectToAction("AccountPage");

                    //Messaggio di eliminazione riuscita
                }
                catch (Exception ex)
                {
                    //Errore database 
                    _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                    return (RedirectToAction("AccountPage"));
                }

            }
            else
            {
                _logger.LogInformation($"Nessun Account loggato {DateTime.Now:HH:mm:ss}");
                return RedirectToAction("AccountPage");
            }
        }
    }
}

