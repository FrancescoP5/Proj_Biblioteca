using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace Proj_Biblioteca.Controllers
{

    public class UtentiController : BaseController
    {
        public UtentiController(IHttpContextAccessor contextAccessor, ILogger<BaseController> logger, LibreriaContext DbContext) : base(contextAccessor, logger, DbContext)
        {
        }

        public async Task<IActionResult> AccountPage()
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Utenti/AccountPage");
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
            UtenteViewModel? UtenteLoggato = await GetUser("Utenti/GestioneRuoli");
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
            UtenteViewModel? UtenteLoggato = await GetUser("Utenti/CambiaRuolo");

            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
            {
                UtenteViewModel? utente = await UtenteViewModel.GetViewModel(_libreria, id);
                if (utente != null)
                {
                    utente.Ruolo = ruolo;

                    try
                    {
                        _libreria.Update(utente);
                        await _libreria.SaveChangesAsync();

                        return Ok();
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
                    return NotFound();
                }
            }
            else
                return Unauthorized();
        }


        [HttpGet]
        public async Task<IActionResult> ListaUtenti(string email)
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Utenti/ListaUtenti");
            if (UtenteLoggato != null && UtenteLoggato.Ruolo == "Admin")
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
                Utente? utente = await _libreria.Utenti.AsNoTracking().FirstOrDefaultAsync(p => p.Email == email && p.Password == Encryption.Encrypt(password) );
                
                if (utente != null)
                {
                    await SetUser(utente.ID,"Utenti/Login");
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
            try
            {

                _libreria.Add(new Utente() { Nome = nome, Email = email, Password = password, DDR = DateTime.Now, Ruolo = "Utente" });

                await _libreria.SaveChangesAsync();

                //Messaggio di riuscita Registrazione
                _logger.LogInformation($"Registrazione riuscita alle ore {DateTime.Now:HH:mm:ss}");

                return await Login(email, password);
            }
            catch (DbUpdateException ex)
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
        public async Task<IActionResult> Disconnect()
        {
            UtenteViewModel? UtenteLoggato = await GetUser("Utenti/Disconnect");
            if(UtenteLoggato!=null)
            _logger.LogInformation($"Utente: {UtenteLoggato.Nome} disconnesso alle ore {DateTime.Now:HH:mm:ss}");
            await SetUser(null, "Utenti/Disconnect");

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
            UtenteViewModel? UtenteLoggato = await GetUser("Utenti/Delete");
            if (UtenteLoggato != null)
            {
                List<Prenotazione> prenotazioni;
                prenotazioni = await _libreria.Prenotazioni.Include(p=>p.Libro).AsNoTracking().Where(p=>p.UtenteID == UtenteLoggato.ID).ToListAsync();

                foreach (Prenotazione p in prenotazioni)
                {
                    try
                    {
                        Libro? libro = p.Libro;
                        _libreria.Remove(p);
                        await _libreria.SaveChangesAsync();

                        libro.Disponibilita++;
                        _libreria.Update(libro);
                        await _libreria.SaveChangesAsync();

                    }
                    catch(DbUpdateException ex)
                    {
                        //Errore database 
                        _logger.LogError($"{ex.ToString()} || {DateTime.Now:HH:mm:ss.ff}");
                        return (RedirectToAction("AccountPage"));
                    }

                }
                try
                {
                    _libreria.Remove(UtenteLoggato);
                    await _libreria.SaveChangesAsync();

                    _logger.LogInformation($"Utente: {UtenteLoggato.Nome} Eliminazione riuscita alle ore {DateTime.Now:HH:mm:ss}");
                    await SetUser(null,"Utenti/Delete");
                    return RedirectToAction("AccountPage");

                    //Messaggio di eliminazione riuscita
                }
                catch(DbUpdateException ex)
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

