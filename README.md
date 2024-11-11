sviluppo di un'applicazione software completa che gestisca un sistema di prenotazione per una biblioteca. 
l'applicazione deve includere la progettazione del database, l'implementazione del backend utilizzando .NET, e la creazione di un'interfaccia utente semplice uutilizzando HTML, CSS e JavaScripts con jQuery

1. Progettazione Database
   Entità Principali: Libri, Utenti, Prenotazioni.
   Relazioni:
     Un libro puo essere prenotato da più utenti (relazione n-n)
     Un utente può avere più prenotazioni.
   Attributi:
     Libri: ID, Titolo, Autore, ISBN, Disponibilita
     Utenti: ID, Nome, Email, Data Di Registrazione
     Prenotazioni: ID, ID_Utente, ID_Libro, Data di Inizio, Data di Fine

2. Sviluppo BackEnd
   Tecnologie Richieste: .NET
   Funzionalita Richieste:
     API RESTful per gestire le operazioni CRUD su libri e utenti.
     Implementare endpoint per effettuare e annullare le prenotazioni.
     Validare i dati in ingresso (ES  email valida per gli utenti).

3. Sviluppo FrontEnd
   Tecnologie Richieste: HTML,CSS,JavaScript con jQuery.
   Interfaccia Utente:
     Pagina Login per gli utenti.
     Pagina principale con l'elenco dei libri disponibili e opzioni per prenotarli.
     Selezione per visualizzare le prenotazioni attive dell'utente.

4. Testing
   Scrivere test unitari per il backend utilizzando strumenti appropiati.
   Eseguire Test funzionali sull'interfaccia utente per garantire che tutte le funzionalita siano operative.
   
5. Documentazione
   Creare una documentazione chiara e concisa per l'API.
   Fornire istruzioni su come installare e avviare l'applicazione.

Criteri Di Valutazione
  Completezza della funzionalità implementata.
  Qualità e chiarezza della progettazione del database.
  Struttura e leggibilità del codice.
  Qualità della documentazione fornita.
   
   
Scadenza Lunedì 18/11   

https://trello.com/invite/b/6731d4f62b4b3939c371922f/ATTI21ab8292208b691e772527e5b5a9f7888290852E/projbiblioteca
