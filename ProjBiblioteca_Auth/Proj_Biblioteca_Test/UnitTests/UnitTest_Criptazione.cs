﻿using Proj_Biblioteca.Utils;

namespace Proj_Biblioteca_Test.UnitTests
{
    [TestClass]
    public class UnitTest_Criptazione
    {


        [TestMethod]
        public void Test_Criptazione()
        {
            string Input = "Example";

            string ExpectedOutput = "4F87FcSElwruptX0nMj06w==";

            string Output = Encryption.Encrypt(Input);

            Assert.AreEqual(Encryption.Encrypt(Input), Output, "Errore, criptazione non stabile");
            Assert.AreEqual(ExpectedOutput, Output, "Attenzione, metodo di criptazione cambiato");
        }

        [TestMethod]
        public void Test_Decriptazione()
        {
            string Input = "4F87FcSElwruptX0nMj06w==";

            string ExpectedOutput = "Example";

            string Output = Encryption.Decrypt(Input);

            Assert.AreEqual(Encryption.Decrypt(Input), Output, "Errore, decriptazione non stabile");
            Assert.AreEqual(ExpectedOutput, Output, "Attenzione, metodo di decriptazione cambiato");
        }

        [TestMethod]
        public void Test_Criptazione_Decriptazione()
        {
            string Input = Encryption.Encrypt("Example");

            string ExpectedOutput = "Example";

            string Output = Encryption.Decrypt(Input);

            Assert.AreEqual(ExpectedOutput, Output, "Errore, criptazione/decriptazione non funzionante");
        }
    }
}
