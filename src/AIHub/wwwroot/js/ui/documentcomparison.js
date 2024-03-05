Dropzone.options.uploadForm  = { 
    paramName: "documentFiles", 
    uploadMultiple: true,
    parallelUploads: 10,
    maxFiles: 10,
    autoDiscover: false,
    autoProcessQueue: true,
    acceptedFiles: ".pdf",
    createImageThumbnails:true,
    previewsContainer: "#file-previews",
    accept: function(files, done) {
        done();
    },
    init: function() {         
        var myDropzone = this;     
        myDropzone.on("sendingmultiple", function(files) {             
            $("#loader").removeClass("d-none");        
        });              
        myDropzone.on("complete", 
        function(files) {                    
            $("#loader").addClass("d-none");                    
        });

    },
    success: function (files, response) {
        if (typeof response === "object") {        
            $("#show-message-result").text(response.message);

            document.getElementById("tabname1").innerText = response.tabName1;
            document.getElementById("tabname2").innerText = response.tabName2;

            if (document.getElementById('pdfUrl1') ==null) {
                var ifrm = document.createElement("iframe");
                ifrm.setAttribute("src", response.pdfUrl1);
                ifrm.setAttribute("id", "pdfUrl1");
                ifrm.setAttribute("width", "100%");
                ifrm.setAttribute("height", "400px");
                document.getElementById("tabcontent1").appendChild(ifrm);
            }

            if (document.getElementById('pdfUrl2') ==null) {
                var ifrm = document.createElement("iframe");
                ifrm.setAttribute("src", response.pdfUrl2);
                ifrm.setAttribute("id", "pdfUrl2");
                ifrm.setAttribute("width", "100%");
                ifrm.setAttribute("height", "400px");
                document.getElementById("tabcontent2").appendChild(ifrm);
            }

        } else {
            try {
                var parsedResponse = JSON.parse(response);
                alert(response);
                $("#show-message-result").val(parsedResponse.message);

                document.getElementById("tabname1").innerText = response.tabName1;
                document.getElementById("tabname2").innerText = response.tabName2;
    
                if (document.getElementById('pdfUrl1') ==null) {
                    var ifrm = document.createElement("iframe");
                    ifrm.setAttribute("src", response.pdfUrl1);
                    ifrm.setAttribute("id", "pdfUrl1");
                    ifrm.setAttribute("width", "100%");
                    ifrm.setAttribute("height", "400px");
                    document.getElementById("tabcontent1").appendChild(ifrm);
                }
    
                if (document.getElementById('pdfUrl2') ==null) {
                    var ifrm = document.createElement("iframe");
                    ifrm.setAttribute("src", response.pdfUrl2);
                    ifrm.setAttribute("id", "pdfUrl2");
                    ifrm.setAttribute("width", "100%");
                    ifrm.setAttribute("height", "400px");
                    document.getElementById("tabcontent2").appendChild(ifrm);
                }
            } catch (e) {
                console.error("Error parsing the response:", e);
            }
        }

        $("#showresult").removeClass("d-none");

        
    },
};
