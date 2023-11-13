// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var contentSafetyDropdown = document.getElementById('contentSafetyDropdown');
var dropdownMenu = document.querySelector('.dropdown-menu');

contentSafetyDropdown.addEventListener('click', function () {
    dropdownMenu.classList.toggle('show');
});