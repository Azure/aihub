Dropzone.options.dropaiimage = { 
    paramName: "imageFile", 
    maxFilesize: 2, // MB
    createImageThumbnails:true,
    previewsContainer: "#file-previews",
    accept: function(file, done) {
        done();
    },
    init: function() {         
        var myDropzone = this;          
        myDropzone.on("sending", function(file) {             
            $("#loader").removeClass("d-none");        
        });              
        myDropzone.on("complete", 
        function(file) {                    
            $("#loader").addClass("d-none");                    
        });
    },
    success: function (file, response) {
        if (typeof response === "object") {        
            $("#show-message-result").text(response.message);

            var ifrm = document.createElement("iframe");
            ifrm.setAttribute("src", response.pdfUrl);
            document.getElementById("pdf-frame").appendChild(ifrm);
        } else {
            try {
                var parsedResponse = JSON.parse(response);
                $("#show-message-result").val(parsedResponse.message);

                var ifrm = document.createElement("iframe");
                ifrm.setAttribute("src", response.pdfUrl);
                document.getElementById("pdf-frame")
            } catch (e) {
                console.error("Error parsing the response:", e);
            }
        }

        $("#showresult").removeClass("d-none");
    },
};