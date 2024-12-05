using Microsoft.AspNetCore.Identity;
using Proj_Biblioteca.DAL;
using Proj_Biblioteca.Data;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.Service;
using Proj_Biblioteca.Utils;
using Proj_Biblioteca.ViewModels;
using System.Data;

namespace Proj_Biblioteca_Test
{
    internal class Manager
    {
        internal static readonly IRepoLibri repoLibri = new FakeRepoLibri();
        internal static readonly IRepoUtenti repoUtenti = new FakeRepoUtenti();
        internal static readonly IRepoPrenotazioni repoPrenotazioni = new FakeRepoPrenotazioni();

        public static readonly LibreriaManager manager = new(repoPrenotazioni,repoLibri,repoUtenti);
    }

    [TestClass]
    public class Test_Servizio_Utenti
    {
        LibreriaManager libreriaManager = Manager.manager;

        [TestMethod]    
        public void Test_Servizio_Login()
        {
            var message = libreriaManager.Utenti().Login("user@example.com", "Esempio").Result;

            Assert.IsTrue(message == "Successo! Verrai reindirizzato a breve..", message);
        }

        [TestMethod]
        public void Test_Servizio_Registrazione()
        {
            Utente utente = new Utente() { UserName = "Esempio", Email="Esempio@gmail.com", PasswordHash = "Pippo50!"};
            var message = libreriaManager.Utenti().Registrazione(utente.UserName,utente.Email,utente.PasswordHash).Result;
            Assert.IsTrue(message == "Successo! Verrai reindirizzato a breve..", message);
        }

        [TestMethod]
        public void Test_Servizio_CambioRuolo()
        {
            Assert.IsTrue(libreriaManager.Utenti().CambiaRuolo("user123", "Utenti").Result);
        }

        [TestMethod]
        public void Test_Servizio_Logout()
        {
           Assert.IsTrue(libreriaManager.Utenti().Disconnect().Result);
        }

        [TestMethod]
        public void Test_Servizio_DeleteAccount()
        {
            Utente utente = new Utente() {UserName = "Esempio", Email = "Esempio@gmail.com", PasswordHash = "Pippo50!" };
            var message = libreriaManager.Utenti().Registrazione(utente.UserName, utente.Email, utente.PasswordHash).Result;
            UtenteViewModel utenteVM = libreriaManager.Utenti().GetViewModels().Result.First(p => p.Email == utente.Email);

            Assert.IsTrue(libreriaManager.Utenti().Delete(utenteVM).Result);
            Assert.IsNull(libreriaManager.Utenti().GetViewModel("aaa").Result);
        }
    }
    

    [TestClass]
    public class Test_Servizio_Libri
    {
        LibreriaManager libreriaManager = Manager.manager;

        [TestMethod]
        public void A() { Assert.IsTrue(true); }
    }

    [TestClass]
    public class Test_Servizio_Prenotazioni
    {
        LibreriaManager libreriaManager = Manager.manager;
        [TestMethod]
        public void A() { Assert.IsTrue(true); }
    }


    public class FakeRepoUtenti : IRepoUtenti
    {
        private List<Utente> _utenti = new List<Utente>
        {
            new Utente { Id = "user123", UserName = "John Doe", Email = "user@example.com" , PasswordHash=Encryption.Encrypt("Esempio") }
        };

        public Task<Utente?> GetUtente(string id)
        {
            return Task.FromResult(_utenti.FirstOrDefault(u => u.Id == id));
        }

        public Task<IEnumerable<Utente>> GetUtenti()
        {
            return Task.FromResult(_utenti as IEnumerable<Utente>);
        }

        public Task<IdentityUserRole<string>> GetUserRole(string id)
        {
            return Task.FromResult(new IdentityUserRole<string>{RoleId="utente"}); // Simuliamo che l'utente abbia sempre il ruolo di Admin
        }

        public Task<Role?> GetRuolo(string ruoloId)
        {
            return Task.FromResult(new Role { Name = "Admin" });
        }

        public Task<Utente?> GetByCredentials(string email, string password)
        {
            return Task.FromResult(_utenti.FirstOrDefault(u => u.Email == email && u.PasswordHash==password));
        }

        public Task<bool> InsertByCredentials(string nome, string email, string password)
        {
            _utenti.Add(new Utente { Id = Guid.NewGuid().ToString(), UserName = nome, Email = email, PasswordHash=Encryption.Encrypt(password)});
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

        public Task<bool> Delete(Utente utente)
        {
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
        }
    }

    internal class FakeRepoLibri : IRepoLibri
    {
        private List<Libro> _libri = new List<Libro>
    {
        new Libro { ID = 1, Titolo = "Libro di Test", Disponibilita = 5 }
    };

        public Task<IEnumerable<Libro>> GetLibri()
        {
            return Task.FromResult(_libri.AsEnumerable());
        }

        public Task<Libro?> GetLibro(int id)
        {
            return Task.FromResult(_libri.FirstOrDefault(l => l.ID == id));
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
            throw new NotImplementedException();
        }
    }

    internal class FakeRepoPrenotazioni : IRepoPrenotazioni
    {
        private List<Prenotazione> _prenotazioni = new List<Prenotazione>();

        public Task<IEnumerable<Prenotazione>> GetPrenotazioni()
        {
            return Task.FromResult(_prenotazioni.AsEnumerable());
        }

        public Task<IEnumerable<Prenotazione>> GetPrenotazioni(string idUtente)
        {
            return Task.FromResult(_prenotazioni.Where(p => p.IdUtente == idUtente));
        }

        public Task<Prenotazione?> GetPrenotazione(int id)
        {
            return Task.FromResult(_prenotazioni.FirstOrDefault(p => p.ID == id));
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
    }
}

