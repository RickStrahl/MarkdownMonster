var mm;
window.MarkdownMonster =  {
    serverUrl: 'http://localhost:5009/',
    isServerAvailable: false,
    lastSendData: null,
    // one time load attempt flag - unset after a prompt has been made
    // recommend you set this true right before an operation that requires access
    tryToLoadMarkdownMonster: false,

    initialize: function() {
        //this.checkServer();
    },
    checkServer: function(onCompleted) {
        var config = { method: "GET"}

        return fetch(mm.serverUrl + "ping",config)
                .then(
                    function(response) {
                        console.log("Markdown Monster: Web Server is available.");
                        var text = response.text();
                        mm.isServerAvailable = true;

                        if (onCompleted)
                            onCompleted(mm);
                    },
                    function(error) {
                        console.warn("Markdown Monster: Web Server is not available.");
                        mm.isServerAvailable = false;

                        if (onCompleted)
                            onCompleted(mm);
                    });
    },
    openNewDocument: function(docText, forceOpen) {
        // check again and if still not open force open with Application Protocol
        this.checkServer(function() {
            if (!mm.isServerAvailable) {
                mm.loadMarkdownMonster();
                return;
            }
            var openCommandLine = "untitled.base64," + btoa(docText);

            var config = { method: "POST", body: JSON.stringify( { operation: "open", data: openCommandLine }), mode: "cors"  };
            fetch(mm.serverUrl + "markdownmonster",config)
                .then(function(response) {
                    console.log("Document created in Markdown Monster.");
                })
                .catch(function(error) {
                    console.warn("Document opening in Markdown Monster failed.");
                });
        });
    },
    loadMarkdownMonster: function() {
        if (confirm("Markdown Monster doesn't appear to be runnnig in Web Server mode.\n" +
                    "We can try to start Markdown Monster for you now, " +
                    "or you can manually start it and ensure `Tools | Web Server` is checked.\n\n" +
                    "Do you want us to try and start Markdown Monster for you?\n" +
                    "(if nothing happens, please start manually)"))
                    {
                        mm.openAppProtocol("markdownmonster:webserver");
                    }
    },
    getActiveDocumentText: function(callback) {
        this.checkServer(function() {
            if (!mm.isServerAvailable) {
                mm.loadMarkdownMonster();
                return;
            }

            var text = "this is the result";

            if(callback)
               callback(text,mm);
        });
    },

    /*
        * opens an application protocol URL without navigating the window
    */
    openAppProtocol: function(url) {
        // this works
        var iframe = document.createElement("iframe");
        iframe.src = url;
        iframe.width = 100;
        iframe.height = 100;
        iframe.style.display = "block";
        document.body.appendChild(iframe);

        setTimeout(function() { document.body.removeChild(iframe)},800);
    },

};
mm = window.MarkdownMonsterWebSocket;
