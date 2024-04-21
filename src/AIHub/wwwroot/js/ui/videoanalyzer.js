Dropzone.options.dropaiimage = {
    paramName: "videoFile",
    maxFilesize: 200, // MB
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
           
            var isrc = document.createElement("source");
            isrc.setAttribute("src", response.video);
            isrc.setAttribute("type", "video/mp4");
            document.getElementById("bigpic").appendChild(isrc);
 
        } else {
            try {
                var parsedResponse = JSON.parse(response);
                $("#show-message-result").val(parsedResponse.message);
           
                var isrc = document.createElement("source");
                isrc.setAttribute("src", parsedResponse.video);
                isrc.setAttribute("type", "video/mp4");
                document.getElementById("bigpic").appendChild(isrc);
 
            } catch (e) {
                console.error("Error parsing the response:", e);
            }
        }
        $("#showresult").removeClass("d-none");
    },
};