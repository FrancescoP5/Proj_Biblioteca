using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;
using Proj_Biblioteca.Data;

namespace Proj_Biblioteca_Test
{
    [TestClass]
    public class UnitTest_Utenti
    {
        [TestMethod]
        public void Test_EmailViewModel()
        {
            string Email_Input = "Example@Example.com";

            string Email_ExpectedOutput = "Example@Example.com";

            UtenteViewModel Utente = new UtenteViewModel() { DDR = new DateTime(2000, 1, 1, 1, 1, 1, 1, 1), Email = Email_Input, ID = 0, Nome = "Example", Ruolo = "Utente" };

            string Email_Output = Utente.Email;


            Assert.AreEqual(Email_ExpectedOutput, Email_Output, "UtenteViewModel Email diverse.");

        }

        [TestMethod]
        public void Test_EmailModel()
        {
            string Email_Input = "Example@Example.com";

            string Email_ExpectedOutput = "Example@Example.com";

            Utente Utente = new Utente() { DDR = new DateTime(2000, 1, 1, 1, 1, 1, 1, 1), Email = Email_Input, ID = 0, Nome = "Example", Ruolo = "Utente", Password = "Example" };

            string Email_Output = Utente.Email;


            Assert.AreEqual(Email_ExpectedOutput, Email_Output, "UtenteViewModel Email diverse.");

        }

        [TestMethod]
        public void Test_Password()
        {
            string Password_Input = "Example";

            string Password_ExpectedOutput = Encryption.Encrypt(Password_Input);

            Utente Utente = new Utente() { DDR = new DateTime(2000, 1, 1, 1, 1, 1, 1, 1), Email = "Example@Example.com", ID = 0, Nome = "Example", Ruolo = "Utente", Password = Password_Input };

            string Password_Output = Utente.Password;

            Assert.AreEqual(Password_ExpectedOutput, Password_Output, "Password non corrispondenti");
        }
    }
}
