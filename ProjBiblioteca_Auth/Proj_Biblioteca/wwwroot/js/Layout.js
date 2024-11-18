

document.querySelectorAll(".TitoloHeader").forEach(item => {
    item.addEventListener("click", () => {
        window.location.href = "/Libro/Elenco";
    })
})


document.querySelectorAll(".Account").forEach(item => {
    item.addEventListener("click", () => {
        window.location.href = "/Utenti/AccountPage";
    })
})

function AggiungiLibri() {
    window.location.href = "/Libro/AggiungiLibro";
}