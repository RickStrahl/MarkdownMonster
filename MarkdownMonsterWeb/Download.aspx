<%@ Page Language="C#" %>
<%@ Import Namespace="System.Net" %>

<%
    //WestWindSiteUtils.LogInfo("/WebMonitor/default.aspx");	
%>
<!DOCTYPE html>
<html>
<head>
    <title>Download Markdown Monster</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    
    <meta name="description" content="West Wind Markdown Monster: An easy to use and extensible Markdown editor for Windows" />
    <meta name="keywords" content="Markdown,Editor,Editing,Weblog,Writing,Documentation" />

    <link href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.min.css" rel="stylesheet" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet" />
    
    <style>
        dt { float: left;width: 90px; font-weight: normal}
        dd { font-size: 100% }
        .btn-info {
            background-color: #4d94d0;            
        }
        .panel {
            min-height: 110px;
        }
    </style>
</head>
<body>
    <div class="banner">        
          
        <span class="banner-title" style="cursor: pointer;" onclick="window.location = './';">
            <img src="Images/MarkdownMonster_Icon_32.png"
                style="height: 28px;"
                alt="Markdown Monster" />
            <span>West Wind Markdown Monster</span>
        </span>      
        <div id="TopMenu" class="right">
            <a href="./">Home</a>
            <%--<a href="https://youtu.be/O5J8mDfVZH8">Video</a>--%>
            <%--<a href="features.aspx">Features</a>--%>
            <a href="download.aspx" class="active">Download</a>
            <a href="http://store.west-wind.com/product/markdown_monster">Buy</a>
            <a href="docs/">Docs</a>
            <%--<a href="pricing.aspx">License</a>--%>
        </div>    
    </div>
    

    <div id="MainContainer" class="background">
        <div id="ContentContainer" class="content" style="padding: 10px 45px;">
            
            <h2>Download West Wind Markdown Monster</h2>
            
            <p>
                You can download the free version of West Wind Markdown Monster and start editing your
                Markdown files like a pro.
            </p>

            <div class="well well-lg">
                <div class="row">
                    <div class=" col-sm-5">
                        
                        <img src="Images/MarkdownMonster_Icon_256.png" style="display: block; margin-bottom: 15px; width: 210px"/>
                        <p><b style="font-family: 'Arial Black'; font-size: 15pt">Markdown Monster</b></p>
                        <dl>                            
                            <dt>Version:</dt>
                            <dd>v<%= Version %> beta</dd>
                                
                            <dt>Released:</dt>
                            <dd><%= ReleaseDate %></dd>

                            <dt>File size:</dt>
                            <dd>3.8 Mb</dd>
                       </dl> 
                                                                        
                    </div>
                    <div class=" col-sm-7">
                        <a class="btn btn-lg btn-info" onclick="window.location.href='http://west-wind.com/files/MarkdownMonsterSetup.exe'">
                            <i class="fa fa-download fa-" style="font-size: 150%;"></i> &nbsp; 
                            Download Installer
                        </a>
                        
                        <div class="small" style="margin-top: 10px">download <a href="javascript:{}" onclick="window.location.href='http://west-wind.com/files/MarkdownMonsterSetup.zip'">Zip file</a></div>
                        <div style="margin-top: 15px;">
                            <div class="fa fa-info-circle" style="font-size: 280%; color: steelblue; float: left;"></div>
                            <div style="margin-left: 50px;">
                                
                                <div class="small">  
                                    <p>
                                    Markdown Monster can be downloaded and evaluated for free, but a <a href="https://store.west-wind.com/product/MARKDOWN_MONSTER">reasonably priced license</a>
                                    must be purchased for continued use. Licenses are per-user, rather than per-machine, so you can use 
                                    Markdown Monster on as many computers you wish with your license. 
                                    </p>
                                    <p>
                                    This download provides a fully functional, non-limited version that includes all of 
                                    Markdown Monster's features.<br />
                                    </p>
                                    <p>
                                    Thanks for playing fair.                                
                                    </p>
                                    
                                </div>

                            </div>
                        </div>
                        
                        <div style="margin-top: 20px;">
                                                    
                        <a href="http://chocolatey.org/packages/MarkdownMonster" style="display:block" >
                                        <img src="images/chocolatey.png" style="width: 170px; margin-top: 10px;" alt="Chocolatey" />                                        
                                    </a>  
                          <p>
                              You can also install directly from Chocolatey's package store:
<%--                                    If you have <a href="https://chocolatey.org/">Chocolatey</a> installed you can also install West Wind WebSurge 
                                    directly from the <a href="https://chocolatey.org/packages/WestwindMarkdownMonster">package repository</a>:--%>
                                    
                                    <pre style="font-size: 10pt; font-family: Consolas, monospace;color: whitesmoke;background: #535353">c:\> choco install MarkdownMonster</pre>                                                          
                                </p>
                            </div>
                    </div>
                </div>                
            </div>
            
            
            <div class="row">
                <div class="col-sm-6">
                    <div class="panel panel-default">
                        <div class="panel-heading">
                            <h5 class="panel-title">System Requirements</h5>
                        </div>
                        <div id="FeatureList" class="panel-body">
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Microsoft Windows Vista or 2008 and newer
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                32 or 64 bit
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Microsoft .NET 4.5 Runtime (<a href="http://smallestdotnet.com/">check</a>)
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="col-sm-6">
                    <div class="panel panel-default" >
                        <div class="panel-heading">
                            <h5 class="panel-title">Installation Instructions</h5>
                        </div>

                        <div id="FeatureList" class="panel-body">
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Download the zip file
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Run the contained <b>MarkdownMonsterSetup.exe</b>
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Follow the installation instructions
                            </div>
                        </div>
                    </div>
                </div>
        </div>
            
            <a name="Licensing"></a>
        <div class="panel panel-default">
            
            <div class="panel-heading">
                    <h5 class="panel-title">Licensing</h5>
            </div>
            <div class="panel-body">
                <p>
                    Although we provide the <a href="https://github.com/RickStrahl/MarkdownMonster">source in the open</a>, 
                    Markdown Monster is licensed software.
                </p>
                <p>
                    Markdown Monster can be downloaded and evaluated for free, but <a href="https://store.west-wind.com/product/markdown_monster">a reasonably 
                    priced license</a> must be purchased for continued use. Licenses are per-user, 
                    rather than per-machine, so you can use Markdown Monster on as many computers 
                    you wish with your license.
                </p>
                <p>
                    Thanks for playing fair.
                 </p>
                <p>
                    
                    <a href="https://store.west-wind.com/product/markdown_monster" class="btn btn-lg btn-primary">
                        <i class="fa  fa-credit-card"></i>
                        Purchase
                    </a>
                </p>
                  
                 <h3>Warranty Disclaimer: No Warranty!</h3>
                <p>IN NO EVENT SHALL THE AUTHOR, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR REDISTRIBUTE THIS PROGRAM AND DOCUMENTATION, BE LIABLE FOR ANY COMMERCIAL, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY TO USE THE PROGRAM INCLUDING, BUT NOT LIMITED TO, LOSS OF DATA OR DATA BEING RENDERED INACCURATE OR LOSSES SUSTAINED BY YOU OR LOSSES SUSTAINED BY THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE WITH ANY OTHER PROGRAMS, EVEN IF YOU OR OTHER PARTIES HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.</p>
                

            </div>
            

        </div>
        <div class="clearfix"></div>            
    </div>
        <div style="height: 50px;"></div>
   </div>

    <nav class="banner" style="font-size: 8pt; padding: 10px; height: 80px; border-top: solid black 4px;
                                                                                                                                                                                                                                                                                                     border-bottom: none;">
        <div class="right">
            created by:<br />
            <a href="http://west-wind.com/" style="padding: 0;">
                <img src="/Images/wwToolbarLogo.png" style="width: 150px;" />
            </a>
        </div>
        &copy; West Wind Technologies, 2000-<%= DateTime.Now.Year %>
    </nav> 
    
    <script>
        (function (i, s, o, g, r, a, m) {
            i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                (i[r].q = i[r].q || []).push(arguments)
            }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
        })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

        ga('create', 'UA-9492219-10', 'west-wind.com');
        ga('send', 'pageview');

    </script>  
</body>

</html>
<script runat="server">
    // This code dynamically updates version and date info
    // whenver the application is restarted
    static string Version
    {
        get
        {
            
            if (_version != null)
                return _version;

            // set default dates for fallback here
            _version = "0.79";
            ReleaseDate = "April 4th, 2015";

            try
            {
                WebClient client = new WebClient();
                string xml = client.DownloadString("http://west-wind.com/files/MarkdownMonster_version.xml");
                Regex regex = new Regex(@"<Version>(.*)<\/Version>");
                MatchCollection matches = regex.Matches(xml);
                if (matches != null && matches.Count > 0)
                {
                    _version = matches[0].Value;
                }

                regex = new Regex(@"<ReleaseDate>(.*)<\/ReleaseDate>");
                matches = regex.Matches(xml);
                if (matches != null && matches.Count > 0)
                {
                    ReleaseDate = matches[0].Value;
                }
            }
            catch
            {
            }

            return _version;
        }
    }
    private static string _version;
    public static string ReleaseDate;
</script>