var mmws;
window.MarkdownMonsterWebSocket =  {    
    connectRetries: 3,
    _activeRetries: 0,
    websocket: null,
    lastSendData: null,     
    // one time load attempt flag - unset after a prompt has been made
    // recommend you set this true right before an operation that requires access
    tryToLoadMarkdownMonster: false,
    startWebSocket:  function(port) {
        websocket = null;
        if (!port)
           port = 5009;

        var wsUri = "ws://127.0.0.1:" + port + "/markdownmonster";
        mmws.websocket = new WebSocket(wsUri);   //  ignore error message in console on connect failure
        websocket = mmws.websocket;
            
        websocket.onopen = function (e) {
            if(mmws.onOpen)
                mmws.onOpen(e);            
            console.log("Markdown Monster socket connected.");
        };                

        websocket.onclose = function (e) {
            if(mmws.onClose)
                mmws.onClose(e);
        };

        websocket.onmessage = function (e) {
            if(mmws.onMessage)
                mmws.onMessage(e);
        };        
        websocket.onerror = function (e) {  
            
            // Try to load MM
            if (mmws.tryToLoadMarkdownMonster && 
                (e.target.readyState === WebSocket.CLOSED || e.target.readyState === WebSocket.CONNECTING) ) {  
                if (mmws.tryToLoadMarkdownMonster &&
                    confirm("Markdown Monster doesn't appear to be runnnig in Web Socket mode.\n" +
                            "We can try to start Markdown Monster for you now, " +
                            "or you can manually start it and ensure `Tools | WebSocket Server` is checked.\n\n" +
                            "Do you want us to try and start Markdown Monster for you?\n" +
                            "(if nothing happens, please start manually)"))
                {          
                    console.warn("Markdown Monster: Socket connection error.");                      
                    mmws.loadMarkdownMonster();
                }
                // always 
                mmws.tryToLoadMarkdownMonster = false;             
            }         
            // Or Retry if count is set            
            else if (!mmws.tryToLoadMarkdownMonster   &&
                     (e.target.readyState === WebSocket.CLOSED || e.target.readyState == WebSocket.CONNECTING)) { 
                if (mmws._activeRetries > 0) {
                    mmws._activeRetries--;
                    console.warn("Markdown Monster: Attempting to reconnect WebSocket.")
                    setTimeout(() => startWebSocket(), 1000);
                }
            }
            else
                console.warn("Markdown Monster: Socket error.");
                
            if(mmws.onError)
                mmws.onError(e);            
        };
    },
    openNewDocument: function(docText) {
        if(!mmws.websocket || 
           mmws.websocket.readyState != WebSocket.OPEN ) {
            
            // open the socket again
            mmws.startWebSocket();

            var delay = mmws.websocket.readyState == WebSocket.CONNECTING ? 500 : 80;
            

            // try again after a short delay
            setTimeout(function() {                
                if (mmws.websocket.readyState == WebSocket.CONNECTING) {
                    var e = {};
                    e.target = {}
                    e.target.readyState = WebSocket.CONNECTING;                    
                    mmws.websocket.onerror(e);
                }
                else
                {
                    mmws.lastSendData = docText;
                    mmws.openNewDocument(docText) ; 
                }                
            },delay);
            return;
        }                       

        if (mmws.onSend)
        {
            var result = mmws.onSend(docText, mmws.websocket);
            if (result && typeof result == "boolean" )
                return;  // don't execute 
        }
        mmws.websocket.send(docText);                            
    },
    onOpen: null,
    onClose: null, 
    onError: null,
    onMessage: null,
    loadMarkdownMonster: function() {
        mmws.loadedFlag = false;
        mmws.openAppProtocol("markdownmonster:websocketserver");
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
mmws = window.MarkdownMonsterWebSocket;