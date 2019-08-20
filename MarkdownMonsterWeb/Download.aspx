<%@ Page Language="C#" %>
<%@ Register TagPrefix="ww" Namespace="Westwind.Web.Markdown" Assembly="Westwind.Web.Markdown" %>
<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Net" %>
<!DOCTYPE html>
<html>
<head>
    <title>Download Markdown Monster</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />    
    <meta name="description" content="Download Markdown Monster: A better Markdown Editor and Weblog Publisher for Windows" />
    <meta name="keywords" content="Markdown,Editor,Editing,Weblog,Writing,Documentation,Windows,Download" />  

    <link href="https://netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.css" rel="stylesheet" />
    <link href="https://netdna.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css" rel="stylesheet" />
    
    <style>
        dt { float: left;width: 90px; font-weight: normal}
        dd { font-size: 100% }        
        .panel {
            min-height: 140px;
        }
        #ContentContainer {
            max-width: 1000px;
        }
       
    </style>
</head>
<body>
    <div class="banner">        
          
        <span class="banner-title" style="cursor: pointer;" onclick="window.location = './';">
            <img src="Images/MarkdownMonster_Icon_32.png"
                style="height: 28px;"
                alt="Markdown Monster" />
            <span>Markdown Monster</span>
        </span>      
        <div id="TopMenu" class="right">
            <a href="./">Home</a>            
            <a href="download.aspx" class="active">Download</a>
            <a href="purchase.aspx" title="Purchase Markdown Monster">Buy</a>
            <a href="docs/" class="hidable" title="Documentation">Docs</a>
            <a href="https://medium.com/markdown-monster-blog/" title="Markdown Monster Weblog">                
                Blog
            </a>
        </div>    
    </div>
    

    <div id="MainContainer" class="background">
        <div id="ContentContainer" class="content" style="padding: 10px 45px;">
            
            <h2>Download Markdown Monster</h2>
            
            <p>
                You can download the free version of West Wind Markdown Monster and start editing your
                Markdown files like a pro.
            </p>

            <div class="well well-lg">
                <div class="row">
                    <div class=" col-sm-5">
                        
                        <img src="Images/MarkdownMonster_Icon_256.png" style="display: block; margin-bottom: 15px; width: 210px"/>
                        <p><b style=" font-size: 1.45em;font-weight: 800">Markdown Monster</b></p>
                        <dl>                            
                            <dt>Version:</dt>
                            <dd>v<%= Version %> 
                                <a style="font-size: 0.8em;padding-left: 15px" 
                                   href="https://github.com/RickStrahl/MarkdownMonster/blob/master/Changelog.md">what's new?</a></dd>
                                
                            <dt>Released:</dt>
                            <dd><%= ReleaseDate %></dd>

                            <dt>File size:</dt>
                            <dd>10 mb</dd>
                       </dl> 
                             
                        <p style="margin-top: 35px;">
                            <a href="purchase.aspx#License" class="btn btn-primary" style="width: 210px">
                                <i class="fa fa-info-circle" style="color: #ddd"></i>&nbsp;
                                License Information
                            </a>
                        </p>
                        <p id="License" >
                            <a id="Licensing" href="purchase.aspx" class="btn btn-primary" style="width: 210px">
                                <i class="fa fa-check" style="color: lightgreen"></i>&nbsp;
                                Buy Markdown Monster
                            </a>
                        </p>
                       

                    </div>
                    <div class=" col-sm-7">
	                    
						<a class="btn btn-lg btn-primary" href="https://west-wind.com/files/MarkdownMonsterSetup.exe" 
                           style=" border-radius: 4px;padding: 12px 20px; font-weight: 600;"
                           title="Download the full Markdown Monster Setup Installation Executable.&#013;This Installer is the recommended way to install Markdown Monster."
                           
                           
                           >
                            <i class="fa fa-download text-success" style="font-size: 1.4em; color: #e2c271; "></i> &nbsp; 
                            Download Markdown Monster
                        </a>
						
	                    <a href="http://www.softpedia.com/get/Office-tools/Text-editors/Markdown-Monster.shtml#status">
		                    <img src="images/Softpedia.png" style="margin: 5px; margin-left: 15px; height: 115px;"/>
	                    </a>
                        
						
                        <div class="small" style="margin-top: 10px">alternates: 
                            <a href="https://west-wind.com/files/MarkdownMonsterSetup.zip" 
                               title="Full Setup exe wrapped in a zip file for those that can't download binaries directly.">Setup Zip</a> | 

                            <a href="https://west-wind.com/files/MarkdownMonsterPortable.zip" 
                               title="Self contained, non-admin installation for Markdown Monster.
Simply unzip into a folder and run MarkdownMonster.exe!

Preferably install into a folder under your User Account folder 
to allow writing of settings that can move with your installation.

If you install in a folder or drive without write permissions, 
settings are stored in the `%appdata%\Markdown Monster` 
user folder.">Portable Zip</a> | 

                            <a href="https://west-wind.com/files/MarkdownMonsterSetup_Latest.exe" 
                               title="Latest pre-release installer that might be slightly ahead of the current release version.">Latest pre-Release</a>
                            <small style="font-size: 0.7em">(<%= LatestVersion %>)</small>
                        </div> 
                        <div style="margin-top: 15px;">
                            <div class="fa fa-info-circle" style="font-size: 280%; color: steelblue; float: left;"></div>
                            <div style="margin-left: 50px;">
                                
                                <div class="small">  
                                    <p>
                                    Markdown Monster can be downloaded and evaluated for free, but a 
                                    <a href="purchase.aspx">reasonably priced license</a>
                                    must be purchased for continued use. <a href="purchase.aspx#License">Licenses</a> are per-user, so you can use 
                                    Markdown Monster on as many computers you wish with your license.
                                    </p>
                                    <p>
                                    Want a <a href="purchase.aspx#Contribute">free license</a>? Contributors who help out with code, feature suggestions, 
                                    documentation or promotion qualify for a free license.
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
                                        <img src="images/chocolatey.png" style="width: 170px; margin-top: 10px;margin-bottom: 5px" alt="Chocolatey" />                                        
                                    </a>  
                          <p>
                              You can also install Markdown Monster from the command line using <a href="https://chocolatey.org/packages/MarkdownMonster">Chocolatey</a>:
                                    <style>
                                        .comment-line { color: forestgreen; }
                                    </style>                                                                 
									<pre style="background: #111;color: #e2e2e2; font-weight: 500; padding: 10px 0 10px 15px;font-size: 1em;">
<code><span class="comment-line"># install markdown monster</span>
c:\> choco install markdownmonster

<span class="comment-line"># upgrade to the latest version</span>
c:\> choco upgrade markdownmonster

<span class="comment-line"># install using the portable, non-admin installer</span>
c:\> choco install markdownmonster.portable
</code></pre>                                                                                                                    
                                </p>
                            </div>
                    </div>
                </div>                
            </div>
            
            
            <div class="row">
                <div class="col-sm-6">
                    <div class="panel panel-default ">
                        <div class="panel-heading">
                            <h5 class="panel-title">System Requirements</h5>
                        </div>
                        <div id="FeatureList" class="panel-body">
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Microsoft Windows 10-7 or Windows 2019-2008R2
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Microsoft .NET Framework <a href="https://dotnet.microsoft.com/download/dotnet-framework">4.6.2 or later</a>
                            </div>   
                            <div>
                                <i class="fa fa-check-circle"></i>
                                <a href="https://git-scm.com/" target="_blank">Git</a> <small>(optional)</small>
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
                                Download the exe or zip file
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Run <b>MarkdownMonsterSetup.exe</b>
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Rock your Markdown like a Pro
                            </div>
                        </div>
                    </div>
                </div>
        </div>
            
       
        <div class="clearfix"></div>      
            
        
    </div>
        
    <nav class="banner banner-bottom" style="font-size: 8pt; margin-top: 100px; padding: 10px; height: 80px; border-top: solid black 4px;border-bottom: none;">
        <div class="right">
            created by:<br />
            <a href="http://west-wind.com/" style="padding: 0;">
                <img src="Images/wwToolbarLogo.png" style="width: 150px;" />
            </a>
            

        </div>
        &copy; West Wind Technologies, 2015-<%= DateTime.Now.Year %>
    </nav> 
   </div>

   <%-- <script src="scripts/highlightjs/highlight.pack.js"></script>
    <link href="scripts/highlightjs/styles/vs2015.css" rel="stylesheet" />--%>

    
<%--    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.13.1/styles/vs2015.min.css">
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/9.13.1/highlight.min.js"></script>--%>

    <script>
        function highlightCode() {
            var pres = document.querySelectorAll("pre>code");
            for (var i = 0; i < pres.length; i++) {
                hljs.highlightBlock(pres[i]);
            }
        }
        highlightCode();
    </script>
    
<% if (!Request.Url.ToString().Contains("localhost"))
   { %>

    <!-- Global Site Tag (gtag.js) - Google Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-9492219-14"></script>
    <script>
        window.dataLayer = window.dataLayer || [];
        function gtag() { dataLayer.push(arguments) };
        gtag('js', new Date());

        gtag('config', 'UA-9492219-14');
    </script>
<% } %>
</body>

</html>
<script runat="server">
    // This code dynamically updates version and date info
    // whenver the application is restarted
    static string Version
    {
        get
        {
            if (_version != null && DateTime.UtcNow.Subtract(_lastAccess).TotalMinutes < 10)
                return _version;
            
            // set default dates for fallback here
            _version = "1.13";
            ReleaseDate = "November 4th, 2018";

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

                _lastAccess = DateTime.UtcNow.AddHours(-1);
                var latestVersion = LatestVersion;

                _lastAccess = DateTime.UtcNow;
            }
            catch
            {
            }

            return _version;
        }
    }
    private static string _version;
    private static DateTime _lastAccess = DateTime.UtcNow;
    public static string ReleaseDate;

    static string LatestVersion
    {
        get
        {
            if (_latestVersion != null && DateTime.UtcNow.Subtract(_lastAccess).TotalMinutes < 5)
                return _latestVersion;

            string path = @"c:\ftp\files\MarkdownMonsterSetup_Latest.exe";
            //string path = @"C:\projects2010\MarkdownMonster\Install\Builds\CurrentRelease\MarkdownMonsterSetup_Latest.exe";


            if (!File.Exists(path))
            {
                _latestVersion = _version;
                return _latestVersion;
            }

            var version = FileVersionInfo.GetVersionInfo(path);
            _latestVersion = version.FileVersion.ToString();
            _lastAccess = DateTime.UtcNow;

            return LatestVersion;
        }
    }

    static string _latestVersion;
</script>