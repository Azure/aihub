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
            $("#bigpic").attr('src', response.image);
        } else {
            try {
                var parsedResponse = JSON.parse(response);
                $("#show-message-result").val(parsedResponse.message);
                $("#bigpic").attr('src',parsedResponse.image);
            } catch (e) {
                console.error("Error parsing the response:", e);
            }
        }
        $("#showresult").removeClass("d-none");
    },
};