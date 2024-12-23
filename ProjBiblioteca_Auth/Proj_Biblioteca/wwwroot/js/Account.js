﻿﻿
const loginForm = document.getElementById("FormAccesso");
const registrationForm = document.getElementById("FormRegistrazione");



function FormRegistrazione() {
    loginForm.classList.add("hidden");
    registrationForm.classList.remove("hidden")
}

function FormLogin() {
    loginForm.classList.remove("hidden");
    registrationForm.classList.add("hidden")
}

$('#local-tf').change(function () {
    $('.toLocalDate').each(function () {
        var utcDate = $(this).data('utcdate');

        var dateValues = utcDate.split("/");

        var localDate = new Date(dateValues[2] + '-' + dateValues[1] + '-' + dateValues[0]);
        $(this).html(localDate.toLocaleDateString());
    });
});

$('#utc-tf').change(function () {
    $('.toLocalDate').each(function () {
        var utcDate = $(this).data('utcdate');
        $(this).html(utcDate+"UTC");
    });
});

$('.RimuoviPrenotazione').click(function () {
    var form = $(this).parent();
    $.ajax({
        url: '/Prenotazioni/RimuoviPrenotazione',
        type: 'post',
        data: form.serialize(),
        success: function () {
            window.location.reload();
        }
    });
});

$('.disconnect').click(function () {
    var token = $(".jsAntiForgeryToken");
    token = $(token).children()
    $.ajax({
        url: '/Utenti/Disconnect',
        type: 'post',
        data:{
            __RequestVerificationToken: token[0].value,
        },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        success: function () {
            window.location.reload();
        }
    });
});

$('.delete').click(function () {
    var token = $(".jsAntiForgeryToken");
    token = $(token).children()
    $.ajax({
        url: '/Utenti/Delete',
        type: 'post',
        data: {
            __RequestVerificationToken: token[0].value,
        },
        contentType: 'application/x-www-form-urlencoded; charset=utf-8',
        success: function () {
            window.location.reload();
        }
    });
});