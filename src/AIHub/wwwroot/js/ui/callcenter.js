$(document).ready(function(){
  $('a.nav-link').bind( "click", function(event) {
    if ( $('a.nav-link').hasClass('active') ) {
      if ( $('div.tab-pane').hasClass('active') ) {

          var findactivetext = document.getElementById("v-pills-tab").querySelector("a.active").id;
          var activatetext = findactivetext.replace('-tab', '');
          var textsel = document.getElementById(activatetext).innerText;
         
          document.getElementById("text").value = textsel;
          
      }
    }
  });
  
  if ( $('a.nav-link').hasClass('active') ) {
    if ( $('div.tab-pane').hasClass('active') ) {
        var text = $('div.tab-pane.fade.active.show')[0].innerText;
        document.getElementById("text").value = text;
    }
  }
});

function submitForm() {

  $("#loader").removeClass("d-none"); 

  // Submit the form
  var form = document.querySelector('form');
  form.submit();
}

window.onload = function () {
  $("#loader").addClass("d-none");
}