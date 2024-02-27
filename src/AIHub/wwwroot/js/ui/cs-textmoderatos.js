function copyText(text) {
    document.getElementById("text").value = text;
}

function submitForm() {

    $("#loader").removeClass("d-none"); 

    // Submit the form
    var form = document.querySelector('form');
    form.submit();
}

window.onload = function () {
    $("#loader").addClass("d-none");
}

function loadExample1() {
    document.getElementById("text").value="Un chino de mierda me ha dado un golpe y me ha destrozado el lateral derecho del coche. Ahora, si quiero salir por ahí tengo que hacerlo por la ventanilla. Encima me dice el desgraciado de él que no tiene seguro.";
}

function loadExample2() {
    document.getElementById("text").value="Si no me concedéis ese préstamo voy a ir a la oficina y le voy a pegar fuego con vosotros dentro.";
}