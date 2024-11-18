using System.Reflection;

namespace Proj_Biblioteca.Data
{
    public class Entity
    {
        //Classe padre di Libri e Utenti

        public int Id { get; set; }

        /*
         * Metodo che permette di valorizzare i parametri di un oggetto attraverso una Dictionary<string(key),string(value)>
         * le key della dictionary devono avere lo stesso nome dei parametri poiché faccio uso delle reflections
         */
        public async void FromDictionary(Dictionary<string, string> riga)
        {
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (riga.ContainsKey(property.Name.ToLower()))
                {
                    object? valore = null;

                    switch (property.PropertyType.Name.ToLower()) //A seconda del tipo del parametro converto il valore associato alla chiave
                    {
                        case "int32":
                            if (int.TryParse(riga[property.Name.ToLower()], out int intVal))
                            {
                                valore = (int?)intVal;
                            }
                            break;

                        case "int64":
                            if (long.TryParse(riga[property.Name.ToLower()], out long longVal))
                            {
                                valore = (long?)longVal;
                            }
                            break;

                        case "double":
                            string doubleString = riga[property.Name.ToLower()].Replace(",", "."); //Converto le virgole in . poichè sql se impostato in lingua italiana usa le virgole per i decimali
                            if (double.TryParse(doubleString, out double doubleVal))
                            {
                                valore = (double?)doubleVal;
                            }
                            break;

                        case "bool":
                            if (bool.TryParse(riga[property.Name.ToLower()], out bool boolVal))
                            {
                                valore = boolVal;
                            }
                            break;

                        case "datetime":
                            if (DateTime.TryParse(riga[property.Name.ToLower()], out DateTime dateVal))
                            {
                                valore = (DateTime?)dateVal;
                            }
                            break;
                        case "string":
                            valore = riga[property.Name.ToLower()];
                            break;

                    }
                    property.SetValue(this, valore);  //Inserisco il valore convertito nel parametro associato
                }
            }
        }
    }
}
