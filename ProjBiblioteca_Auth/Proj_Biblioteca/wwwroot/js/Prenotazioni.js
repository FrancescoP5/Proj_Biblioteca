var date;
var dataMassima;

function CambioData(maxGiorni) {
    var datafine = document.getElementById("dataFine");
    var dataInizio = document.getElementById("dataInizio");


    date = new Date(dataInizio.value);
    dataMassima = new Date();


    datafine.value = date.toISOString().split('T')[0];
    datafine.min = date.toISOString().split('T')[0];
    
    datafine.max = date.addDays(maxGiorni).toISOString().split('T')[0];

}

Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}
