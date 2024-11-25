using Proj_Biblioteca.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_Biblioteca.Models
{
    public class Utente
    {
        public int ID { get; set; }


        public string Nome { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public string Ruolo { get; set; }


        [DataType(DataType.Password)]
        public string Password 
        {
            get
            {
                return _password;
            }
            set
            {
                _password = Encryption.Encrypt(value);
            }
        }

        [NotMapped]
        private string _password;
        //[NotMapped]
        //public string Password
        //{
        //    get { return Encryption.Decrypt(PasswordStored); }
        //    set { PasswordStored = Encryption.Encrypt(value); }
        //}


        [DataType(DataType.DateTime)]
        public DateTime DDR { get; set; }

    }
}
