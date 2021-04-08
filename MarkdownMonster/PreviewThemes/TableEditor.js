var page = {
    tableData: {
        isPreviewActive: false,
        activeCell: { row: 0, column: 0, isHeader: true },
        headers: [
            "Header 1",
            "Header 2:",
            "Header 3"
        ],
        rows: [
            [
                "Column 1",
                "Column 2",
                "Column 3"
            ]            
        ]
    },
    dotnet: null, 
    mousePos: { 
        x: -1, 
        y: -1, 
        // row 0: Header, row -1: not set
        row: -1,
        col: -1
    },
    workElement: null,
    initialize: function() {        
        // if(page.tableData.rows.length < 3) {
        //     for (let i = 0; i < 1000; i++) {
        //         var rowValues = [
        //             "Column 1  Row " + i,
        //             "Column 2  Row " + i,
        //             "Column 3  Row " + i,
        //             "Column 4  Row " + i,
        //             "Column 5  Row " + i
        //         ];
        //         page.tableData.rows[i] = rowValues;
        //     }
        // }
        var div$ = $("#RenderWrapper");
        page.workElement = document.createElement("div");

        // handle tab insertion and up down key navigation
        div$.on("keydown","td textarea,th textarea",page.keydownHandler);

        div$.on("keyup","th textarea", page.headerAlignment);        
        setTimeout(function() { $("th textarea").trigger("keyup"); },10);

        div$.on("keyup","textarea", page.autogrowTextAreas); 
        div$.on("change", "textarea", function () {            
            if (page.dotnet) {                                
                var pos = page.idToPos(this.id);                                     
                page.tableData.activeCell.row = pos.row;
                page.tableData.activeCell.column = pos.column;                
                page.dotnet.RefreshPreview(pos);
            }
        });
        div$.on("contextmenu","textarea",function(e) {                   
            var textBox = e.target;
            if (textBox.tagName != "TEXTAREA") return;
            var pos = page.idToPos(textBox.id);
            
            if (page.dotnet)     
            {                
                page.dotnet.ShowContextMenu(pos);
            }
            else {
                alert(JSON.stringify(pos));
            }
        });    
        div$.on("focus","textarea",function() {                   
             this.select(); 
        });        
        $(document).on("mouseleave",function() {
            if(page.dotnet)
                page.dotnet.UpdateTableData(page.parseTable(true));                
        });
    },
    keydownHandler: function(e) {
        var text$ = $(this);
        var id = this.id;
        
        var pos = page.idToPos(id);
        if(pos.row == -1) return;   
        
        // console.log("keyDown Key: " + e.keyCode);
        
        // tab end of list insertion
        if (!e.shiftKey && e.keyCode === 9 && this.parentNode.tagName !== "TH") {                 
            // find next td            
            var $next = $(this).parent().next();                
            if ($next.length > 0)  // not last textarea
                return;

            // find next tr                     
            var tr$ = $(this).parent().parent().next();
            if (tr$.length > 0)  // not last tr
                return;

            var clonedTr$ = $(this).parent().parent().clone();            
            clonedTr$.find("textarea").each(function(i) {                 
                var pos = page.idToPos(  this.id);
                var row = pos.row + 1;
                this.id = "id_" + row + "_" + pos.column;                
                this.value = "";
            });            
            $("tbody").append(clonedTr$);
        }

        // ctrl-enter
        else if (e.keyCode == 13 && e.ctrlKey) {
            if (page.dotnet)
               page.dotnet.KeyboardCommand("Ctrl-Enter");
        }
        // down key navigates
        else if (e.keyCode == 40) 
        {
            var hasReturns = this.value.indexOf("\n") > 0;
            if (hasReturns && !e.ctrlKey) return null;  // line breaks - don't use arrows

            // move to new row
            var newRow = pos.row + 1;
            var newId = "#id_" + newRow + "_" + pos.column;
            $(newId).focus();

            return false;
        }

        // up key navigation
        else if (e.keyCode == 38) 
        {                    
            var hasReturns = this.value.indexOf("\n") > 0;
            if (pos.row < 1 || (hasReturns && !e.ctrlKey) ) return true;  

            var newRow = pos.row - 1;
            var newId = "#id_" + newRow + "_" + pos.column;
            $(newId).focus();
            return false;
        }               
    },

    headerAlignment: function() {
        var el$ = $(this);
        var text = el$.val();
        
        var pos = page.idToPos(this.id);
        if (pos.row !== 0) return;

        var col = pos.column + 1;
        var cols$ = $("#RenderWrapper thead th:nth-child(" +col + ") textarea," +
                      "#RenderWrapper tbody td:nth-child(" + col + ") textarea");

        cols$.removeClass("center-align");
        cols$.removeClass("right-align");
                 
        if(text[text.length-1] == ":" && text[0] == ":") {                               
            cols$.addClass("center-align");
        }  
        if(text[text.length-1] == ":") {                               
            cols$.addClass("right-align");
        }   
        
    },
    renderTable: function(tableData, focusLocation) {
        try {
            if (typeof tableData === "string")
                page.tableData = JSON.parse(tableData);
            if(typeof focusLocation === "string")
                focusLocation = JSON.parse(focusLocation);
                
            var html = "<table>\n";
            var headers = page.tableData.headers;

            // row 0
            if (headers && headers.length > 0) {
                html += "<thead>\n<tr>"

                for (let i = 0; i < headers.length; i++) {
                    var colText = headers[i];              
                    var c =  i;  
                    html += "<th><textarea id='id_0_" + c  + "'>" + page.encodeText(colText) + "</textarea></th>";                
                }
                html += "</tr>\n</thead>"
            }

            // content rows are 1 based to account for row ids
            var rows = page.tableData.rows;
            if(rows && rows.length > 0)
            {
                html += "<tbody>\n"

                for (let rowIdx = 0; rowIdx < rows.length; rowIdx++) {
                    var rowArray = rows[rowIdx];
                    html += "<tr>\n"
        
                    for (var colIdx = 0; colIdx < rowArray.length; colIdx++) {
                        var colText = rowArray[colIdx];
                        var r = rowIdx * 1 + 1;
                        var c = colIdx * 1;                                     
                        html += "<td><textarea id='id_" + r  + "_" +  c +  "'>" + page.encodeText(colText) + "</textarea></td>\n";                                
                    }

                    html += "</tr>\n"
                }
                html += "</tbody>"
            }

            html += "</table>";
            $("#RenderWrapper").html(html);
            
            if (focusLocation) {
                if(focusLocation.isHeader)
                {
                    $("th textarea").first().focus();
                }else {
                    var row = focusLocation.row + 1;
                    var sel = "#id_" + row + "_" + focusLocation.column;            
                    $(sel).focus();
                }
            }

            setTimeout(function() { 
                $("th textarea").trigger("keyup"); 
                $("#RenderWrapper textarea").each(function() {            
                    page.autogrowTextAreas(this);
                });  
            },20);
            
        } catch(ex)   
        {
             alert("RenderTable Error:\n" + ex.message);
        }
    },
    parseTable: function(asJson) {
        var td = {
            activeCell: { row: 3, column: 1},
            headers: [],
            rows: [] 
        };
        $("#RenderWrapper thead th textarea").each(function(i) {
            td.headers[i] = this.value;
        });
        $("#RenderWrapper tbody tr").each(function(i) {            
            var row = [];
            $(this).find("textarea").each(function(x) {
                row[x] = this.value;
            });
            td.rows[i] = row;            
        });
        page.tableData = td;   

        // strip out trailing lines
        for (let i = td.rows.length-1; i > -1 ; i--) {
            var row = td.rows[i];
            var nonBlank =  false;
            for (let x = 0; x < row.length; x++) {
                if (row[x]) {
                    nonBlank = true;
                    break;
                }                                
            }
            if (nonBlank)  break;
            td.rows.pop();
        }
        
        if (asJson)     
        {
            var json = JSON.stringify(td);            
            return json;
        }
        return td;
    },

    idToPos: function(id) {
        var pos = { row: -1, column: -1};

        var id = id.replace("id_","");
        var tokens = id.split("_");
        if (tokens.length < 2) return pos;
           
        pos.row = tokens[0] * 1;
        pos.column = tokens[1] * 1;
        return pos;
    },
    autogrowTextAreas: function autogrowTextAreas(sel) {
        var element = sel;         
        if (sel["currentTarget"])   // event object
            element = this;           
        
        var text = element.value;      
        if(!text)   {
            element.rows = 1;
            return;
        }
    
        var lines = 1 + (text.match(/\n/g) || []).length;        
        element.rows = lines;
        
        return true;
    },
    encodeText: function(text) {
        page.workElement.innerText = text;
        return page.workElement.innerHTML.replace(/<br>/g,"\n");        
    }
};  // page

page.initialize();
page.renderTable();

function InitializeInterop(dotnet, tableDataJson) {        
    page.dotnet = dotnet;
    page.tableData = JSON.parse(tableDataJson);
    page.renderTable();

    setTimeout(function() {
        var first = $("th textarea");
        if (first.length > 0)
            setTimeout(function() { first[0].focus(); }, 320);
    });

    return page;
}
