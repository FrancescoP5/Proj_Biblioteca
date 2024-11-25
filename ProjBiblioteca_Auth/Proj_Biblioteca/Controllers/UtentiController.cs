using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Proj_Biblioteca.Controllers
{

    public class UtentiController : BaseController
    {
        public UtentiController(IHttpContextAccessor contextAccessor, ILogger<BaseController> logger) : base(contextAccessor, logger)
        {
        }

        public async Task<IActionResult> AccountPage()
        {
            Utente? UtenteLoggato = await GetUser("Utenti/AccountPage");
            IEnumerable<Prenotazione>? prenotazioni = null;

            

            if (ViewData.ContainsKey("Messaggio"))
                ViewData["Messaggio"] = TempData["Messaggio"];
            else
                ViewData.Add("Messaggio", TempData["Messaggio"]);

            if (UtenteLoggato != null)
            {
                if (UtenteLoggato.Ruolo == "Admin")
                {
                    string apiUrl = "https://localhost:7139/Prenotazioni/ElencoPrenotazioni/"+UtenteLoggato.ID;


                    using (var httpClient = new HttpClient())
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                        string prenotazioniCrypted = await response.Content.ReadAsStringAsync();
                        string prenotazioniJson = Encryption.Decrypt(prenotazioniCrypted);
                        prenotazioni = JsonSerializer.Deserialize<IEnumerable<Prenotazione>>(prenotazioniJson);

                        if (response.IsSuccessStatusCode)
                        {
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
                    string apiUrl = "https://localhost:7139/Prenotazioni/GetPrenotazioni/" + UtenteLoggato.ID;


                    using (var httpClient = new HttpClient())
                    {

                        HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                        string prenotazioniCrypted = await response.Content.ReadAsStringAsync();
                        string prenotazioniJson = Encryption.Decrypt(prenotazioniCrypted);
                        prenotazioni = JsonSerializer.Deserialize<IEnumerable<Prenotazione>>(prenotazioniJson);

                        if (response.IsSuccessStatusCode)
                        {
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

        public async Task<IActionResult> GestioneRuoli()
        {
            Utente? UtenteLoggato = await GetUser("Utenti/GestioneRuoli");
            if(UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                return View();
            }
            else
            {
                return RedirectToAction("AccountPage");
            }
        }

        [HttpPut]
        public async Task<IActionResult> CambiaRuolo(int id, string ruolo)
        {
            Utente? UtenteLoggato = await GetUser("Utenti/CambiaRuolo");

            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                Utente utente = /*(Utente)await DAOUtente.GetInstance().Find(id);*/ null;
                if (utente != null)
                {
                    utente.Ruolo = ruolo;
                    if (true /*await DAOUtente.GetInstance().Update(utente)*/)
                        return Ok();
                    else
                        return BadRequest();
                }
                else
                {
                    return NotFound();
                }
            }
            else
                return Unauthorized();
        }


        [HttpGet]
        public async Task<IActionResult> ListaUtenti(string email)
        {
            Utente? UtenteLoggato = await GetUser("Utenti/ListaUtenti");
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {

                List<Utente> utenti = /*(await DAOUtente.GetInstance().ListaUtenti(email)).Cast<Utente>().ToList();*/ null;
                if (utenti.Count > 0)
                {
                    return Json(utenti);
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                return Unauthorized();
            }
        }

        // ~/Utenti/Login/{email}{password}
        /*
         * controlla email e password e fa loggare un utente
         */
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {

            _logger.LogInformation($"Tentativo di accesso alle ore {DateTime.Now:HH:mm:ss}");

            if (MailAddress.TryCreate(email, out _))//check della validita email
            {
                Utente? utente = /*(Utente)await DAOUtente.GetInstance().Login(email, password);*/ null;

                
                if (utente != null)
                {
                    SetUser(utente.ID,"Utenti/Login");
                    return RedirectToAction("AccountPage");
                }
                else
                {
                    TempData["Messaggio"] = "Credenziali Errate, riprovare il Login";
                    return RedirectToAction("AccountPage");
                }
                

            }
            TempData["Messaggio"] = "Errore nel Login riprovare..";
            return RedirectToAction("AccountPage" );
        }

        // ~/Utenti/Registrazione/{nome}{email}{password}
        /*
         * Controlla nome, email e password inseriti e crea un account
         */
        [HttpPost]
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

            if (true/*await DAOUtente.GetInstance().Registrazione(nome, email, password)*/)
            {
                //Messaggio di riuscita Registrazione
                _logger.LogInformation($"Registrazione riuscita alle ore {DateTime.Now:HH:mm:ss}");

                return await Login(email, password);
            }
            else
            {
                //Messaggio di registrazione Fallita
                _logger.LogInformation($"Registrazione fallita alle ore {DateTime.Now:HH:mm:ss}");
                TempData["Messaggio"] = "Errore, Registrazione Fallita riprovare.";
                return RedirectToAction("AccountPage" );
            }

        }

        // ~/Utenti/Disconnect
        /*
         * Disconnette l'utente loggato
         */
        [HttpPost]
        public async Task<IActionResult> Disconnect()
        {
            Utente? UtenteLoggato = await GetUser("Utenti/Disconnect");
           _logger.LogInformation($"Utente: {UtenteLoggato.Nome} disconnesso alle ore {DateTime.Now:HH:mm:ss}");
            SetUser(null,"Utenti/Disconnect");
            
            return RedirectToAction("AccountPage");
        }


        // ~/Utenti/Delete
        /*
         * Elimina l'account dell'utente loggato
         * e resetta tutte le prenotazioni
         */
        [HttpPost]
        public async Task<IActionResult> Delete()
        {
            Utente? UtenteLoggato = await GetUser("Utenti/Delete");
            if (UtenteLoggato != null)
            {
                List<Prenotazione> prenotazioni;
                prenotazioni = /*(await DAOUtente.GetInstance().PrenotazioniUtente(UtenteLoggato)).Cast<Prenotazione>().ToList();*/ null;

                foreach (Prenotazione p in prenotazioni)
                {
                    if (true /*await DAOUtente.GetInstance().RimuoviPrenotazione(p) == false*/)
                    {
                        _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Reset Prenotazioni fallito {DateTime.Now:HH:mm:ss}");
                        return (RedirectToAction("AccountPage"));
                    }
                }
                if (true /*await DAOUtente.GetInstance().Delete(UtenteLoggato.Id)*/)
                {
                    


                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Eliminazione riuscita alle ore {DateTime.Now:HH:mm:ss}");
                    SetUser(null,"Utenti/Delete");
                    return RedirectToAction("AccountPage");

                    //Messaggio di eliminazione riuscita
                }
                else
                {
                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Eliminazione fallita alle ore {DateTime.Now:HH:mm:ss}");
                    return RedirectToAction("AccountPage");
                    //Messaggio di eliminazione fallita
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

