using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Proj_Biblioteca.Models;
using System.Security.Claims;
using System.Security.Principal;
namespace Proj_Biblioteca.Data
{
    public class DAOUtente : IDAO
    {
        //Metodi CRUD per gli Utenti

        private Database db;

        private DAOUtente()
        {
            db = Database.GetInstance();
        }

        #region Singleton
        private static DAOUtente instance = null;
        public static DAOUtente GetInstance()
        {
            if (instance == null)
                instance = new DAOUtente();
            return instance;
        }
        #endregion


        public async Task<bool> Delete(int id)
        {
            bool del = false;
            try
            {
                del = await db.Update($"delete from Utenti where ID = {id}");
            }
            catch
            {
                Console.WriteLine("Errore rimozione Utente");
            }
            return del;
        }


        public async Task<List<Entity>> ListaUtenti(string email)
        {
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            List<Entity> res = new List<Entity>();
            try
            {
                rows = await db.Read($"select * from Utenti where email LIKE '%{email}%'");
                if (rows == null)
                {
                    Console.WriteLine("Errore nessun Utente");
                    return null;
                }
                foreach (Dictionary<string, string> item in rows)
                {
                    Utente e = new Utente();
                    e.FromDictionary(item);
                    res.Add(e);
                }
                return res;
            }
            catch
            {
                Console.WriteLine("Errore ricerca utenti");
                return null;
            }
        }

        public async Task<Entity> Login(string email, string password)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            try
            {
                res = await db.ReadOne($"select * from Utenti where email = '{email.Replace("'", "''")}' AND password = HASHBYTES('SHA2_512','{(password.Replace("'", "''"))}') ");

                if (res!= null && res.Count > 0)
                {
                    Utente e = new Utente();
                    e.FromDictionary(res);
                    return e;
                }
                else
                {
                    Console.WriteLine("Nessun utente trovato!");
                }
            }
            catch
            {
                Console.WriteLine("Errore ricerca utente");
            }
            return null;
        }

        public async Task<bool> Registrazione(string nome, string email, string password)
        {
            return await db.Update($"Insert into Utenti " +
                      $"(Nome, Email, Password, DDR, Ruolo ) " +
                      $"values " +
                      $"('{nome.Replace("'", "''")}', '{email.Replace("'", "''")}', HASHBYTES('SHA2_512','{password.Replace("'", "''")}'), '{DateTime.UtcNow:yyyy-dd-MM HH:mm:ss}', 'Utente' ); ");
        }

        public async Task<List<Entity>> ReadAll()
        {

            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            List<Entity> res = new List<Entity>();
            try
            {
                rows = await db.Read($"select * from Utenti");
                foreach (Dictionary<string, string> item in rows)
                {
                    Utente e = new Utente();
                    e.FromDictionary(item);
                    res.Add(e);
                }
                return res;
            }
            catch
            {
                Console.WriteLine("Errore ricerca utente");
                return null;
            }
        }

        public async Task<bool> Update(Entity e)
        {
            if (e != null)
            {
                if (e is Utente)
                {
                    Utente l = (Utente)e;
                    try
                    {
                        //TODO: Validazione dati in ingresso
                        await db.Update($"Update Utenti Set " +
                                  $"Ruolo = '{l.Ruolo.Replace("'","''")}' " +
                                  $"Where id = {l.Id} ");
                        return true;
                    }
                    catch
                    {
                        Console.WriteLine("errore nella modifica del utente");
                    }
                }
            }
            return false;
        }

        public Task<bool> Insert(Entity e)
        {
            throw new NotImplementedException();
        }

        public async Task<Entity> Find(int id)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            try
            {
                if(id!= null)
                    res = await db.ReadOne($"select * from Utenti where ID = {id}");
                Utente e = new Utente();
                e.FromDictionary(res);
                return e;
            }
            catch
            {
                Console.WriteLine("Errore ricerca utente");
                return null;
            }
        }

        public async Task<Entity> FindPrenotazione(int id)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            try
            {
                if(id!=null)
                    res = await db.ReadOne($"select * from Prenotazioni where ID = {id}");
                if (res == null)
                    return null;

                Prenotazione e = new Prenotazione();


                foreach (var k in res)
                {
                    if (k.Key.ToLower() == "id_libro")
                        e.Libro = (Libro)DAOLibro.GetInstance().Find(int.Parse(k.Value)).Result;

                    if (k.Key.ToLower() == "id_utente")
                        e.Utente = (Utente)DAOUtente.GetInstance().Find(int.Parse(k.Value)).Result;
                }

                e.FromDictionary(res);
                return e;
            }
            catch
            {
                Console.WriteLine("Errore ricerca prenotazione");
                return null;
            }
        }

        public async Task<bool> RimuoviPrenotazione(Prenotazione prenotazione)
        {
            bool del = false;
            try
            {
                del = await db.Update($"delete from Prenotazioni where ID = {prenotazione.Id}");
                await db.Update($"Update Libri Set Disponibilita = {++prenotazione.Libro.Disponibilita} WHERE ID = {prenotazione.Libro.Id}");
            }
            catch
            {
                Console.WriteLine("Errore rimozione Prenotazione");
            }
            return del;
        }

        public async Task<bool> AggiungiPrenotazione(Utente UtenteLoggato, Libro libro, DateTime inizio, DateTime fine)
        {
            try
            {
                //TODO: Validazione dati in ingresso
                if (inizio > DateTime.UtcNow && inizio < fine && libro.Disponibilita > 0 && fine <= inizio.AddDays(libro.PrenotazioneMax+1)) // check della data e della disponibilita
                {
                    await db.Update($"Insert into Prenotazioni " +
                                      $"(ID_Utente, ID_Libro, DDI, DDF) " +
                                      $"values " +
                                      $"( {UtenteLoggato.Id}, {libro.Id}, '{inizio:yyyy-dd-MM HH:mm:ss}', '{fine:yyyy-dd-MM HH:mm:ss}' ); ");

                    await db.Update($"Update Libri Set Disponibilita = {--libro.Disponibilita} WHERE ID = {libro.Id}");
                    return true;
                }
                return false;
            }
            catch
            {
                Console.WriteLine("errore nell'inserimento della prenotazione");
                return false;
            }
        }

        public async Task<List<Entity>> ElencoPrenotazioni()
        {
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            List<Entity> res = new List<Entity>();
            try
            {
                rows = await db.Read($"select * from Prenotazioni ");
                foreach (Dictionary<string, string> item in rows)
                {
                    Libro l = new Libro();
                    Utente u = new Utente();
                    if(rows==null)
                    {
                        Console.WriteLine("Errore nessuna Prenotazione");
                        return null;
                    }
                    foreach (var k in item)
                    {
                        if (k.Key.ToLower() == "id_libro")
                            l = (Libro)DAOLibro.GetInstance().Find(int.Parse(k.Value)).Result;

                        if (k.Key.ToLower() == "id_utente")
                            u = (Utente)DAOUtente.GetInstance().Find(int.Parse(k.Value)).Result;
                    }


                    Prenotazione e = new Prenotazione();
                    e.FromDictionary(item);

                    e.Utente = u;
                    e.Libro = l;

                    res.Add(e);
                }
                return res;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Errore ricerca prenotazione: "+ex);
                return null;
            }
        }

        public async Task<List<Entity>> PrenotazioniUtente(Utente utente)
        {
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            List<Entity> res = new List<Entity>();
            try
            {
                rows = await db.Read($"select * from Prenotazioni Where ID_Utente = {utente.Id} ");
                foreach (Dictionary<string, string> item in rows)
                {
                    Libro l = new Libro();

                    foreach (var k in item)
                    {
                        if (k.Key.ToLower() == "id_libro")
                            l = (Libro)DAOLibro.GetInstance().Find(int.Parse(k.Value)).Result;
                    }


                    Prenotazione e = new Prenotazione();
                    e.FromDictionary(item);

                    e.Utente = (Utente) await Find(utente.Id);
                    e.Libro = l;

                    res.Add(e);
                }
                return res;
            }
            catch
            {
                Console.WriteLine("Errore ricerca prenotazione");
                return null;
            }
        }
    }
}
