
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
    $.ajax({
        url: '/Utenti/Disconnect',
        type: 'post',
        success: function () {
            window.location.reload();
        }
    });
});

$('.delete').click(function () {
    $.ajax({
        url: '/Utenti/Delete',
        type: 'post',
        success: function () {
            window.location.reload();
        }
    });
});
