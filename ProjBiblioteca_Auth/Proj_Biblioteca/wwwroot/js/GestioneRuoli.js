
$('.ricerca').click(function () {
    var form = $(this).parent();
    var data = new FormData(form.target)

    $.ajax({
        url: '/Utenti/ListaUtenti',
        type: 'GET',
        dataType: 'json',
        data: form.serialize(),
        success: function (data) {
            var utenti = '<tr class="table-columns"><th>ID</th><th>Utente</th><th>Email</th><th>Ruolo</th></tr>';
            $.each(data, function (index, value) {
                console.log(value);
                utenti += '<tr class="table-values">';
                utenti += '<th>' + value.id + '</th>';
                utenti += '<th>' + value.nome + '</th>';
                utenti += '<th>' + value.email + '</th>';
                utenti += '<th>';
                utenti +=   '<form method=\"post\" onsubmit=\"return false\">';
                utenti +=   '<input name=\"id\" value=\"'+value.id+'\" hidden/>';
                utenti +=     '<select id=\"ruolo\" name=\"ruolo\">';
                utenti +=         '<option value=\"Admin\">';
                utenti +=             'Admin';
                utenti +=         '</option>'
                utenti +=         '<option value =\"Utente\"'+(value.ruolo=="Utente"?"selected":"")+'>';
                utenti +=             'Utente'
                utenti +=         '</option>';
                utenti +=     '</select>';
                utenti +=     '<button type=\"submit\" class=\"salva\">Salva</button>';
                utenti +=   '</form>';
                utenti += '</th>';
                utenti += '</tr>';
            });
            $(".table").html(utenti);


            $('.salva').click(function () {
                var form = $(this).parent();
                var data = new FormData(form.target)
                $.ajax({
                    url: '/Utenti/CambiaRuolo',
                    type: 'PUT',
                    data: form.serialize(),

                    success: function () {
                        form.removeClass('error');
                        form.addClass('success');
                    },
                    error: function () {
                        form.removeClass('succsess');
                        form.addClass('error');
                    }
                });
            });

        },
        error: function () {
            var utenti = '<tr class="table-columns"><th>ID</th><th>Utente</th><th>Email</th><th>Ruolo</th></tr>';
            $(".table").html(utenti);
        }
    });
});

