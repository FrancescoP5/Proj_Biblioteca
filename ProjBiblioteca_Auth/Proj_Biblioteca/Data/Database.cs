using System.Data.SqlClient;

namespace Proj_Biblioteca.Data
{
    public class Database
    {
        private SqlConnection Connection { get; set; }
        private static Database instance = null;

        private string nomeDB = "master";

        private Database(string nomeDB, string server = "localhost")
        {
            this.nomeDB = nomeDB;

            Connection = new SqlConnection($"Server={server};Integrated security=true;");
            //connessione al db master
            try
            {//controllo di esistenza del db richiesto
                Connection.Open();
                SqlCommand cmd = new SqlCommand($"USE {nomeDB}", Connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {//se il db non esiste, genera un eccezione, allora lo creiamo
                Console.WriteLine("creazione db in corso");
                string query = $"CREATE DATABASE {nomeDB}";
                SqlCommand cmd = new SqlCommand(query, Connection);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand($"USE {nomeDB}", Connection);
                cmd.ExecuteNonQuery();
                Connection.Close();
                //connessione al nostro db
                Connection = new SqlConnection($"Server={server};Database={nomeDB};Integrated security=true;");
                try
                { //creazione delle tabelle
                    CreaTabelle();
                }
                catch
                {
                    Console.WriteLine("Errore durante la creazione delle tabelle: " + ex.Message);
                }
                //il db non era già creato in precedenza
            }
            finally
            {
                Connection.Close();//precauzione
                                   //aggiorna la connessione da master a db corrente se necessario
                Connection = new SqlConnection($"Server=localhost;Database={nomeDB};Integrated security=true;");
            }
        }

        public static Database GetInstance()
        {
            if (instance == null)
                instance = new Database("Proj_Biblioteca");

            return instance;
        }

        //converte la risposta di una query in una lista di dictionary
        //ogni elemento della lista è associato ad un record
        //ogni chiave della dictionary è associato al nome della colonna e il valore al valore della colonna
        //pocihé la nel metodo fromdictionary di entity usiamo questi dictionary,
        //allora è importante che i nomi dei parametri abbiano lo stesso nome delle colonne della tabella associata
        public async Task<List<Dictionary<string, string>>> Read(string query)
        {
            List<Dictionary<string, string>> ris = new List<Dictionary<string, string>>();

            using (SqlConnection conn = new SqlConnection(Connection.ConnectionString))
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand(query, conn);
                using (SqlDataReader dr = cmd.ExecuteReaderAsync().Result)
                {
                    while (dr.ReadAsync().Result)
                    {
                        Dictionary<string, string> riga = new Dictionary<string, string>();

                        for (int i = 0; i < dr.FieldCount; i++)
                        {
                            var columnName = dr.GetName(i).ToLower();
                            object columnValue = dr.GetValue(i);

                            riga.Add(columnName, columnValue.ToString());

                        }

                        ris.Add(riga);
                    }
                }
            }

            return ris;
        }

        public async Task<Dictionary<string, string>> ReadOne(string query)
        {
            try
            {
                return (await Read(query))[0];
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> Update(string query)
        {
            try
            {
                await Connection.OpenAsync();
                //Console.WriteLine("connessione a:" + Connection.Database);

                SqlCommand cmd = new SqlCommand(query, Connection);
                //Statistiche(Connection,query);

                int affette = await cmd.ExecuteNonQueryAsync();

                //Console.WriteLine("Righe affette: " + affette);

                return affette > 0;
            }
            catch (SqlException e)
            {
                //Console.WriteLine(e.Message);
                //Console.WriteLine($"\nQUERY ERRATA:\n{query}");

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Errore generico" + "\n" + e.Message);
                return false;
            }
            finally
            {
                await Connection.CloseAsync();
            }
        }

        public void Statistiche(SqlConnection connection, string query = "")
        {
            Console.WriteLine($"{connection.State}\t {connection.Database}\t {connection.DataSource}");

        }

        private void CreaTabelle()
        {
            //query creazione tabelle
            Update("create table Libri " +
                    "( " +
                    "ID INT PRIMARY KEY IDENTITY(1,1), " +
                    "Titolo VARCHAR(200), " +
                    "Autore VARCHAR(200), " +
                    "ISBN DECIMAL(13,0), " +
                    "PrenotazioneMax INT, " +
                    "Disponibilita INT, " +
                    "); ").Wait();

            //Generazione dei libri
            Update("INSERT INTO Libri " +
                "(Titolo, Autore, PrenotazioneMax, ISBN ,Disponibilita) " +
                "VALUES " +
                "('Il nome della rosa', 'Umberto Eco', 10, 9788806202205, 10), " +
                "('1984', 'George Orwell', 10, 9788807811967, 5), " +
                "('La coscienza di Zeno', 'Italo Svevo', 10, 9788806210927, 8), " +
                "('I promessi sposi', 'Alessandro Manzoni', 10, 9788804604118, 12), " +
                "('Il piccolo principe', 'Antoine de Saint-Exupéry', 10, 9788884517882, 15), " +
                "('Don Chisciotte della Mancia', 'Miguel de Cervantes', 10, 9788871646820, 6), " +
                "('Cent''anni di solitudine', 'Gabriel García Márquez', 10, 9788806205695, 4), " +
                "('Il gatto e il diavolo', 'Gianni Rodari', 10, 9788804539845, 18), " +
                "('La divina commedia', 'Dante Alighieri', 10, 9788804806790, 7), " +
                "('Il Maestro e Margherita', 'Mikhail Bulgakov', 10, 9788804662282, 9); ").Wait();

            Update("create table Utenti " +
                    "( " +
                    "ID INT PRIMARY KEY IDENTITY(1,1), " +
                    "Nome VARCHAR(200), " +
                    "constraint AK_NOME UNIQUE(NOME), " +
                    "Email VARCHAR(320), " +
                    "constraint AK_EMAIL UNIQUE(Email), " +
                    "Password VARCHAR(200), " +
                    "DDR DATETIME, " +
                    "); ").Wait();

            //ACCOUNT ADMIN
            Update("Insert into Utenti " +
                   "(Nome, Email, Password, DDR) " +
                   "values " +
                  $"('Admin', 'Admin@Admin.com', HASHBYTES('SHA2_512','admin'), '{DateTime.UtcNow:yyyy-dd-MM HH:mm:ss}'); ").Wait();

            Update("create table Prenotazioni " +
                    "( " +
                    "ID INT PRIMARY KEY IDENTITY(1,1), " +
                    "ID_Utente INT, " +
                    "ID_Libro INT, " +
                    "DDI DATETIME, " +
                    "DDF DATETIME, " +
                    "FOREIGN KEY (ID_Utente) REFERENCES Utenti(ID) " +
                    "ON DELETE CASCADE ON UPDATE CASCADE, " +
                    "FOREIGN KEY (ID_Libro) REFERENCES Libri(ID) " +
                    "ON DELETE CASCADE ON UPDATE CASCADE " +
                    "); ").Wait();
        }
    }
}
