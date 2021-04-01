var page = {
    tableData: {
        activeCell: { row: 3, column: 1},
        headers: [
            "Column 1",
            "Colum 2:"
        ],
        rows: [
            [
                "Col Text 1 1",
                "Col Text 1 2",
            ],
            [
                "Col Text 2 1",
                "Col Text 2 2",
            ],
            [
                "Col Text 3 1",
                "Col Text 3 2",
            ]
        ],

    },
    dotnet: null, 
    mousePos: { x: 0, y: 0 },
    workElement: null,
    initialize: function() {
        page.workElement = document.createElement("div");

        $(document).on("keydown","td textarea, th textarea",page.tabHandler);

        $(document).on("keyup","th textarea", page.headerAlignment);        
        $("th textarea").trigger("keyup");

        $(document).on("blur",function(e) {
            alert('doc blur');
        });

        $(document).on("mousemove",function(e) {
            page.mousePos.x = e.clientX;
            page.mousePos.y = e.clientY;
        });
        $(document).on("contextmenu",function(e) {       
            if (page.dotnet)     
                page.dotnet.ShowContextMenu(page.mousePos);
        });
    },
    tabHandler: function(e) {        
        if (e.keyCode !== 9) return;
        if (this.parentNode.tagName == "TH") return ;

        if (!e.shiftKey) {                 
            // find next td
            var $next = $(this).parent().next();                
            if ($next.length > 0)
                return;

            // find next tr
            var $next = $(this).parent().parent().next();                
            if ($next.length > 0)
                return;

            var clonedTr$ =  $(this).parent().parent().clone();
            clonedTr$.find("textarea").val("");
            $("tbody").append(clonedTr$);
        }
    },
    headerAlignment: function() {
        console.log('header alignment')
        var el$ = $(this);
        var text = el$.val();

        if (text.length < 2)
            return;

        var id = this.id;
        var tokens = id.split("_");
        for (let i = 0; i < tokens.length; i++) {
            tokens[i] = tokens[i].replace("row","").replace("col","");
        }   
        var col = tokens[1] * 1;

        var cols$ = $("tbody td:nth-child(" + col + ") textarea,"  +
                      "thead th:nth-child(" + col + ") textarea");
        cols$.removeClass("center-align");
        cols$.removeClass("right-align");

        
        if(text[text.length-1] == ":" && text[0] == ":") {                               
            cols$.addClass("center-align");
        }  
        if(text[text.length-1] == ":") {                               
            cols$.addClass("right-align");
        }   
        
    },
    renderTable: function() {
        var html = "<table>\n";
        var headers = page.tableData.headers;

        if (headers && headers.length > 0) {
            html += "<thead>\n<tr>"

            for (let i = 0; i < headers.length; i++) {
                var colText = headers[i];              
                var c =  i  + 1;  
                html += "<th><textarea id='row0_col" + c  + "'>" + page.encodeText(colText) + "</textarea></th>";                
            }
            html += "</tr>\n</thead>"
        }

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
                    var c = colIdx * 1 + 1;                    
                    html += "<td><textarea id='row" + r  + "_col" +  c +  "'>" + page.encodeText(colText) + "</textarea></td>\n";                                
                }

                html += "</tr>\n"
            }
            html += "</tbody>"
        }

        html += "</table>";

        $("#RenderWrapper").html(html);
        
        if (page.tableData.activeCell) {
            var sel = "#row" + page.tableData.activeCell.row + "_col" + page.tableData.activeCell.column;
            console.log(sel);
            $(sel).focus();
        }
    },    
    encodeText: function(text) {
        page.workElement.innerText = text;
        return page.workElement.innerHTML;
    }



};  // page



page.initialize();
page.renderTable();




function InitializeInterop(dotnet, tableDataJson) {        
    page.dotnet = dotnet;
    page.tableData = JSON.parse(tableDataJson);
    page.renderTable();

    
}
