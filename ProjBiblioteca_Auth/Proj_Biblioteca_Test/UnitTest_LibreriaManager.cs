using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Proj_Biblioteca.DAL;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Service;
using Proj_Biblioteca.Utils;
using Proj_Biblioteca.ViewModels;
using System.Data;
using System.Linq.Expressions;

namespace Proj_Biblioteca_Test
{
    internal class Manager
    {
        internal static readonly IRepoLibri repoLibri = new FakeRepoLibri();
        internal static readonly IRepoUtenti repoUtenti = new FakeRepoUtenti();
        internal static readonly IRepoPrenotazioni repoPrenotazioni = new FakeRepoPrenotazioni();

        public static readonly LibreriaManager manager = new(repoPrenotazioni, repoLibri, repoUtenti);
    }

    [TestClass]
    public class Test_Servizio_Utenti
    {
        readonly LibreriaManager libreriaManager = Manager.manager;

        [TestMethod]
        public void Test_Login()
        {
            var message = libreriaManager.Utenti().Login(new LoginViewModel() { Email = "user@example.com", Password = "Esempio" }).Result;

            Assert.IsTrue(message == "Successo! Verrai reindirizzato a breve..", message);
        }

        [TestMethod]
        public void Test_Registrazione()
        {
            Utente utente = new() { UserName = "Esempio", Email = "Esempio@gmail.com", PasswordHash = "Pippo50!" };
            var message = libreriaManager.Utenti().Registrazione(new RegistrazioneViewModel() { Nome = utente.UserName, Email = utente.Email, Password = utente.PasswordHash, ConfermaPassword = utente.PasswordHash }).Result;
            Assert.IsTrue(message == "Successo! Verrai reindirizzato a breve..", message);
        }

        [TestMethod]
        public void Test_CambioRuolo()
        {
            Assert.IsTrue(libreriaManager.Utenti().CambiaRuolo("user123", "Utenti").Result);
        }

        [TestMethod]
        public void Test_Logout()
        {
            Assert.IsTrue(libreriaManager.Utenti().Disconnect().Result);
        }

        [TestMethod]
        public void Test_DeleteAccount()
        {
            Utente utente = new() { UserName = "Esempio", Email = "Esempio@gmail.com", PasswordHash = "Pippo50!" };
            var message = libreriaManager.Utenti().Registrazione(new RegistrazioneViewModel() { Nome = utente.UserName, Email = utente.Email, Password = utente.PasswordHash, ConfermaPassword = utente.PasswordHash }).Result;
            UtenteViewModel utenteVM = libreriaManager.Utenti().GetViewModels().Result.First(p => p.Email == utente.Email);

            Assert.IsTrue(libreriaManager.Utenti().Delete(utenteVM).Result);
            Assert.IsNull(libreriaManager.Utenti().GetViewModel("aaa").Result);
        }
    }


    [TestClass]
    public class Test_Servizio_Libri
    {
        readonly LibreriaManager libreriaManager = Manager.manager;

        [TestMethod]
        public void Test_Lettura()
        {
            var libri = libreriaManager.Libri().Elenco(null,null).Result;

            Assert.IsNotNull(libri);
            Assert.AreEqual(2, libri.Item1.Count());
        }

        [TestMethod]
        public void Test_Scrittura()
        {
            var libro = new Libro() { Autore = "es", Disponibilita = 1, ID = 101, ISBN = 1, PrenotazioneMax = 1, Titolo = "es" };

            Assert.IsTrue(libreriaManager.Libri().Aggiungi(libro).Result);

            Assert.AreEqual(libro, libreriaManager.Libri().FindLibro(libro.ID).Result);
        }
    }

    [TestClass]
    public class Test_Servizio_Prenotazioni
    {
        readonly LibreriaManager libreriaManager = Manager.manager;

        [TestMethod]
        public void Test_Lettura()
        {
            var prenotazioniJson = libreriaManager.Prenotazioni().GetPrenotazioni("user123").Result;
            var prenotazioni = JsonConvert.DeserializeObject<IEnumerable<Prenotazione?>>(Encryption.Decrypt(prenotazioniJson));
            Assert.IsNotNull(prenotazioni);
            Assert.AreEqual(1, prenotazioni.Count());
        }

        [TestMethod]
        public void Test_Scrittura()
        {
            var prenotazione = new Prenotazione() { ID = 101, DDF = DateTime.UtcNow, DDI = DateTime.UtcNow, UtenteId = "user123", LibroID = 2 };

            CodiceStato codice = libreriaManager.Prenotazioni().AggiungiPrenotazione(new AddPrenotazioneViewModel() {IdLibro = prenotazione.LibroID, IdUtente = prenotazione.UtenteId, Inizio = prenotazione.DDI.ToString(), Fine = prenotazione.DDF.ToString(), TimeOffset = 0, ClientTime = 0 }).Result;
            Assert.AreEqual(CodiceStato.Ok, codice);
        }

        [TestMethod]
        public void Test_Scrittura_LibroGiaPrenotato()
        {
            var prenotazione = new Prenotazione() { ID = 101, DDF = DateTime.UtcNow, DDI = DateTime.UtcNow, UtenteId = "user123", LibroID = 1 };
            CodiceStato codice = libreriaManager.Prenotazioni().AggiungiPrenotazione(new AddPrenotazioneViewModel() { IdLibro = prenotazione.LibroID, IdUtente = prenotazione.UtenteId, Inizio = prenotazione.DDI.ToString(), Fine = prenotazione.DDF.ToString(), TimeOffset = 0, ClientTime = 0 }).Result;
            Assert.AreEqual(CodiceStato.Errore_Client, codice);
        }

        [TestMethod]
        public void Test_Scrittura_DatiInsufficienti()
        {
            var prenotazione = new Prenotazione() { ID = 101, DDF = DateTime.UtcNow, DDI = DateTime.UtcNow, UtenteId = "user123", LibroID = 2 };
            CodiceStato codice = libreriaManager.Prenotazioni().AggiungiPrenotazione(new AddPrenotazioneViewModel() { IdLibro = null, IdUtente = prenotazione.UtenteId, Inizio = prenotazione.DDI.ToString(), Fine = prenotazione.DDF.ToString(), TimeOffset = 0, ClientTime = 0 }).Result;
            Assert.AreEqual(CodiceStato.Dati_Insufficienti, codice);
        }
    }

    public class FakeRepoUtenti : IRepoUtenti
    {
        public List<Utente> _utenti =
        [
            new Utente { Id = "user123", UserName = "John Doe", Email = "user@example.com" , PasswordHash="Esempio" }
        ];
        
        public FakeRepoUtenti()
        {
            foreach(var utente in _utenti)
            {
                utente.PasswordHash = Encryption.HashPassword(utente.PasswordHash??"", utente);
            }
        }

        public Task<Utente?> GetUtente(string id)
        {
            return Task.FromResult(_utenti.FirstOrDefault(u => u.Id == id));
        }

        public Task<IEnumerable<Utente?>> GetUtenti()
        {
            return Task.FromResult(_utenti as IEnumerable<Utente?>);
        }

        public Task<IdentityUserRole<string>?> GetUserRole(string id)
        {
            return Task.FromResult(new IdentityUserRole<string> { RoleId = "utente" } ?? null ); // Simuliamo che l'utente abbia sempre il ruolo di Admin
        }

        public Task<Role?> GetRuolo(string ruoloId)
        {
            return Task.FromResult(new Role { Name = "Admin" } ?? null);
        }

        public Task<Utente?> GetByCredentials(string email, string password)
        {

            Utente? utente = _utenti.FirstOrDefault(u => u.Email == email);
            if (utente == null) return Task.FromResult((Utente?)null);

            var verifica = Encryption.VerifyPassword(password,utente);
            if (verifica == "Verificato") return Task.FromResult((Utente?)utente);

            return Task.FromResult((Utente?)null);
        }

        public Task<bool> InsertByCredentials(string nome, string email, string password)
        {
            Utente utente = new() { Id = Guid.NewGuid().ToString(), UserName = nome, Email = email, PasswordHash = password };
            utente.PasswordHash = Encryption.HashPassword(password,utente);
            _utenti.Add(utente);
            return Task.FromResult(true);
        }

        public Task<bool> Login(Utente utente)
        {
            return Task.FromResult(true); // Simuliamo sempre un login con successo
        }

        public Task<bool> Logout()
        {
            return Task.FromResult(true); // Simuliamo sempre un logout con successo
        }

        public Task<bool> Delete(Utente? utente)
        {
            if (utente == null)
                return Task.FromResult(false);

            return Task.FromResult(_utenti.Remove(utente));
        }

        public Task<bool> CambiaRuolo(string id, string ruolo)
        {
            var utente = _utenti.FirstOrDefault(u => u.Id == id);
            if (utente == null) return Task.FromResult(false);
            // Cambia ruolo (non implementato in questo esempio, ma lo simuliamo come un successo)
            return Task.FromResult(true);
        }

        public Task<bool> Insert(Utente utente)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(Utente utente)
        {
            throw new NotImplementedException();
        }

        public Task<int> Save()
        {
            return Task.FromResult(1);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class FakeRepoLibri : IRepoLibri
    {
        public List<Libro> _libri =
        [
            new Libro { ID = 1, Titolo = "Libro di Test", Disponibilita = 5 },
            new Libro { ID = 2, Titolo = "Libro di Test", Disponibilita = 5 }
        ];

        public Task<IEnumerable<Libro?>> GetListAsync(Expression<Func<Libro, bool>>? filters = null, int? page = null, int? pageSize = null)
        {
            IQueryable<Libro> query = _libri.AsQueryable().Where(filters ?? (x => true));


            return Task.FromResult(query.AsEnumerable<Libro?>());
        }

        public Task<Libro?> GetAsync(Expression<Func<Libro, bool>> filters)
        {
            IQueryable<Libro> query = _libri.AsQueryable();
            return Task.FromResult(query.FirstOrDefault(filters));
        }

        public Task<int> PageCountAsync(int pageSize, Expression<Func<Libro, bool>>? filters = null)
        {
            pageSize = pageSize <= 0 ? 1 : pageSize;
            IQueryable<Libro> query = _libri.AsQueryable();


            int totalCount = query.Count(filters ?? (x => true));

            return Task.FromResult((int)Math.Ceiling((double)totalCount / pageSize));

        }

        public Task<bool> Insert(Libro libro)
        {
            _libri.Add(libro);
            return Task.FromResult(true);
        }

        public Task<bool> Update(Libro libro)
        {
            var existingLibro = _libri.FirstOrDefault(l => l.ID == libro.ID);
            if (existingLibro != null)
            {
                existingLibro.Titolo = libro.Titolo;
                existingLibro.Disponibilita = libro.Disponibilita;
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<bool> Delete(Libro libro)
        {
            _libri.Remove(libro);
            return Task.FromResult(true);
        }

        public Task<int> Save()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    internal class FakeRepoPrenotazioni : IRepoPrenotazioni
    {
        public List<Prenotazione> _prenotazioni =
        [
            new Prenotazione() {ID = 1, DDF = DateTime.UtcNow, DDI = DateTime.UtcNow, UtenteId= "user123", LibroID=1 }
        ];

        public Task<IEnumerable<Prenotazione?>> GetListAsync(Expression<Func<Prenotazione, bool>>? filters = null, int? page = null, int? pageSize = null)
        {
            IQueryable<Prenotazione> query = _prenotazioni.AsQueryable().Where(filters ?? (x => true));


            return Task.FromResult(query.AsEnumerable<Prenotazione?>());
        }

        public Task<Prenotazione?> GetAsync(Expression<Func<Prenotazione, bool>> filters)
        {
            IQueryable<Prenotazione> query = _prenotazioni.AsQueryable();
            return Task.FromResult(query.FirstOrDefault(filters));
        }

        public Task<int> PageCountAsync(int pageSize, Expression<Func<Prenotazione, bool>>? filters = null)
        {
            pageSize = pageSize <= 0 ? 1 : pageSize;
            IQueryable<Prenotazione> query = _prenotazioni.AsQueryable();


            int totalCount = query.Count(filters ?? (x => true));

            return Task.FromResult((int)Math.Ceiling((double)totalCount / pageSize));

        }

        public Task<bool> Insert(Prenotazione prenotazione)
        {
            _prenotazioni.Add(prenotazione);
            return Task.FromResult(true);
        }

        public Task<bool> Delete(Prenotazione prenotazione)
        {
            _prenotazioni.Remove(prenotazione);
            return Task.FromResult(true);
        }

        public Task<bool> Update(Prenotazione utente)
        {
            throw new NotImplementedException();
        }

        public Task<int> Save()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Prenotazione?>> GetListAsync(int page, int pageSize)
        {
            IQueryable<Prenotazione> query = _prenotazioni.AsQueryable();


            return Task.FromResult(query.AsEnumerable<Prenotazione?>());
        }

        public Task<IEnumerable<Prenotazione?>> GetListAsync(Expression<Func<Prenotazione, bool>> filters, int page, int pageSize)
        {
            IQueryable<Prenotazione> query = _prenotazioni.AsQueryable().Where(filters ?? (x => true));


            return Task.FromResult(query.AsEnumerable<Prenotazione?>());
        }

        public Task<IEnumerable<Prenotazione?>> GetListAsync(Expression<Func<Prenotazione, bool>> filters, Expression<Func<Prenotazione, object>> ordina, bool ordina_desc, int page, int pageSize)
        {
            IQueryable<Prenotazione> query = _prenotazioni.AsQueryable().Where(filters ?? (x => true));


            return Task.FromResult(query.AsEnumerable<Prenotazione?>());
        }
    }
}

