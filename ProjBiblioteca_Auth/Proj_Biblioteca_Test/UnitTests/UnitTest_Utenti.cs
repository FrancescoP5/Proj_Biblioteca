﻿using Proj_Biblioteca.Models;
using Proj_Biblioteca.ViewModels;


namespace Proj_Biblioteca_Test.UnitTests
{
    [TestClass]
    public class UnitTest_Utenti
    {
        [TestMethod]
        public void Test_EmailViewModel()
        {
            string Email_Input = "Example@Example.com";

            string Email_ExpectedOutput = "Example@Example.com";

            UtenteViewModel Utente = new() { DDR = new DateTime(2000, 1, 1, 1, 1, 1, 1, 1), Email = Email_Input, Id = "0", Nome = "Example", Ruolo = "Utente" };

            string Email_Output = Utente.Email;


            Assert.AreEqual(Email_ExpectedOutput, Email_Output, "UtenteViewModel Email diverse.");

        }

        [TestMethod]
        public void Test_EmailModel()
        {
            string Email_Input = "Example@Example.com";

            string Email_ExpectedOutput = "Example@Example.com";

            Utente Utente = new() { DDR = new DateTime(2000, 1, 1, 1, 1, 1, 1, 1), Email = Email_Input, Id = "0", UserName = "Example", PasswordHash = "Example" };

            string Email_Output = Utente.Email;


            Assert.AreEqual(Email_ExpectedOutput, Email_Output, "UtenteViewModel Email diverse.");

        }


    }
}
