using Proj_Biblioteca.Models;

namespace Proj_Biblioteca.Data
{
    public class DAOLibro : IDAO
    {
        //Metodi CRUD per i Libri 
        private Database db;

        private DAOLibro()
        {
            db = Database.GetInstance();
        }

        #region Singleton
        private static DAOLibro instance = null;
        public static DAOLibro GetInstance()
        {
            if (instance == null)
                instance = new DAOLibro();
            return instance;
        }
        #endregion

        public async Task<bool> Delete(int id)
        {
            bool del = false;
            try
            {
                del = await db.Update($"delete from Libri where ID = {id}");
            }
            catch
            {
                Console.WriteLine("Errore rimozione Libro");
            }
            return del;
        }

        public async Task<Entity> Find(int id)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            try
            {
                res = await db.ReadOne($"select * from Libri where ID = {id}");
                Libro e = new Libro();
                e.FromDictionary(res);
                return e;
            }
            catch
            {
                Console.WriteLine("Errore ricerca libro");
                return null;
            }
        }

        public async Task<bool> Insert(Entity e)
        {
            if (e != null)
            {
                if (e is Libro)
                {
                    Libro l = (Libro)e;
                    try
                    {
                        //TODO: Validazione dati in ingresso
                        await db.Update($"Insert into Libri " +
                                  $"(Titolo, Autore, PrenotazioneMax, ISBN, Disponibilita) " +
                                  $"values " +
                                  $"( '{l.Titolo.Replace("'", "''")}', '{l.Autore.Replace("'", "''")}', {l.PrenotazioneMax}, {l.ISBN}, {l.Disponibilita} ); ");
                        return true;
                    }
                    catch
                    {
                        Console.WriteLine("errore nell'inserimento del libro");
                    }
                }
            }
            return false;
        }

        public async Task<List<Entity>> ReadAll()
        {

            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            List<Entity> res = new List<Entity>();
            try
            {
                rows = await db.Read($"select * from Libri");
                foreach (Dictionary<string, string> item in rows)
                {
                    Libro e = new Libro();
                    e.FromDictionary(item);
                    res.Add(e);
                }
                return res;
            }
            catch
            {
                Console.WriteLine("Errore ricerca libro");
                return null;
            }
        }

        public async Task<bool> Update(Entity e)
        {
            if (e != null)
            {
                if (e is Libro)
                {
                    Libro l = (Libro)e;
                    try
                    {
                        //TODO: Validazione dati in ingresso
                        await db.Update($"Update Libri Set " +
                                  $"Titolo = '{l.Titolo.Replace("'", "''")}', " +
                                  $"Autore = '{l.Autore.Replace("'", "''")}', " +
                                  $"ISBN = {l.ISBN}, " +
                                  $"PrenotazioneMax = {l.PrenotazioneMax}, " +
                                  $"Disponibilita = {l.Disponibilita} " +
                                  $"Where id = {l.Id} ");
                        return true;
                    }
                    catch
                    {
                        Console.WriteLine("errore nella modifica del libro");
                    }
                }
            }
            return false;
        }
    }
}
