$(window).on('load', function () {
    var dataInizio = document.getElementById("dataInizio");

    var dataInizioUtc = $('#dataInizio').data('utcdate');
    var datetime = Date.now();

    var localdate = (new Date(datetime)).toLocaleDateString().split("/");
    dataInizio.min = (new Date(localdate[2]+'-'+localdate[1]+'-'+localdate[0])).toISOString().split("T")[0];
});

Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}
function CambioData(maxGiorni) {
    var datafine = document.getElementById("dataFine");
    var dataInizio = document.getElementById("dataInizio");


    var date = new Date(dataInizio.value);
    var dataMassima = new Date();


    datafine.value = date.toISOString().split('T')[0];
    datafine.min = date.toISOString().split('T')[0];
    
    datafine.max = date.addDays(maxGiorni).toISOString().split('T')[0];

}

$(".prenota").click(function () {
    var form = $(this).parent();

    var timeOffset = $('#timeOffset');
    timeOffset.val((new Date()).getTimezoneOffset() / 60);

    var clientTime = $('#clientTime');
    clientTime.val((new Date()).getHours());


    $.ajax({
        url: "/Prenotazioni/AggiungiPrenotazione",
        type: "POST",
        data: form.serialize(),
        success: function () {
            window.location.href = "/Utenti/AccountPage";
        },
        error: function (msg) {
            var errorMsg = "<h4>" + msg.responseText + "</h4>";
            $(".errorMsg").html(errorMsg);
        }
    });
});
