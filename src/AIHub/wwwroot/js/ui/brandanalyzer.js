function submitForm() {

    $("#loader").removeClass("d-none"); 

    // Submit the form
    var form = document.querySelector('form');
    form.submit();
}

window.onload = function () {
    $("#loader").addClass("d-none");
}