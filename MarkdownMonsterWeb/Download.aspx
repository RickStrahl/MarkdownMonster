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
    <meta name="description" content="Download Markdown Monster: A better Markdown Editor and Weblog Publisher for Windows" />
    <meta name="keywords" content="Markdown,Editor,Editing,Weblog,Writing,Documentation,Windows,Download" />
    
    <!-- Global Site Tag (gtag.js) - Google Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-9492219-14"></script>
    <script>
        window.dataLayer = window.dataLayer || [];
        function gtag() { dataLayer.push(arguments) };
        gtag('js', new Date());

        gtag('config', 'UA-9492219-14');
    </script>
  

    <link href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.css" rel="stylesheet" />
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
            <a href="download.aspx" class="active">Download</a>
            <a href="http://store.west-wind.com/product/order/markdown_monster">Buy</a>
            <a href="docs/" class="hidable">Docs</a>
            <a href="https://medium.com/markdown-monster-blog/" title="Markdown Monster Weblog">                
                Blog
            </a>
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
                        <p><b style=" font-size: 1.45em;font-weight: 800">Markdown Monster</b></p>
                        <dl>                            
                            <dt>Version:</dt>
                            <dd>v<%= Version %> 
                                <a style="font-size: 0.8em;padding-left: 15px" 
                                   href="https://github.com/RickStrahl/MarkdownMonster/blob/master/Changelog.md">what's new?</a></dd>
                                
                            <dt>Released:</dt>
                            <dd><%= ReleaseDate %></dd>

                            <dt>File size:</dt>
                            <dd>9.5 mb</dd>
                       </dl> 
                                                                        
                    </div>
                    <div class=" col-sm-7">
	                    
						<a class="btn btn-lg btn-info" href="https://west-wind.com/files/MarkdownMonsterSetup.exe">
                            <i class="fa fa-download fa-2x" style="font-size: 0.8em; "></i> &nbsp; 
                            Download Markdown Monster
                        </a>
						
	                    <a href="http://www.softpedia.com/get/Office-tools/Text-editors/Markdown-Monster.shtml#status">
		                    <img src="images/Softpedia.png" style="margin: 5px; margin-left: 15px; height: 115px;"/>
	                    </a>
                        
						
                        <div class="small" style="margin-top: 10px">alternates: <a href="https://west-wind.com/files/MarkdownMonsterSetup.zip" title="Full Setup exe wrapped in a zip file for those that can't download binaries directly.">Setup Zip</a> | 
                                                                                <a href="https://west-wind.com/files/MarkdownMonsterPortable.zip" title="Fully self contained folder structure for Markdown Monster that can run without installation. Adds some limitations: No .md file association, no global command line access and Addins may not install if running out of a non-privileged folder.">Portable Zip</a> </div>
                        <div style="margin-top: 15px;">
                            <div class="fa fa-info-circle" style="font-size: 280%; color: steelblue; float: left;"></div>
                            <div style="margin-left: 50px;">
                                
                                <div class="small">  
                                    <p>
                                    Markdown Monster can be downloaded and evaluated for free, but a <a href="https://store.west-wind.com/product/order/markdown_monster">reasonably priced license</a>
                                    must be purchased for continued use. Licenses are per-user, so you can use 
                                    Markdown Monster on as many computers you wish with your license.
                                    </p>
                                    <p>
                                    Want a <b>free license</b>? Contributors who help out with code, feature suggestions, 
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
                              You can also install Markdown Monster from Chocolatey's package store:
<%--                                    If you have <a href="https://chocolatey.org/">Chocolatey</a> installed you can also install West Wind WebSurge 
                                    directly from the <a href="https://chocolatey.org/packages/WestwindMarkdownMonster">package repository</a>:--%>
                                    
                                    <pre style="font-size: 10pt; font-family: Consolas, monospace;color: whitesmoke;background: #535353">c:\> choco install markdownmonster</pre>                                                    
									
									<p>To update an existing installation:</p>
	                        
									<pre style="font-size: 10pt; font-family: Consolas, monospace;color: whitesmoke;background: #535353">c:\> choco upgrade markdownmonster</pre>                                                    
                              
                                    <p>You can also use the <a href="https://chocolatey.org/packages/MarkdownMonster.Portable">portable, non-admin installer</a>:

                                    <pre style="font-size: 10pt; font-family: Consolas, monospace;color: whitesmoke;background: #535353">c:\> choco install markdownmonster.portable</pre>                                                          
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
                                Microsoft Windows 10-7 or Windows 2016-2008R2
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Internet Explorer 11
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Microsoft .NET 4.6+ <small>(4.6.2 recommended)
                                <a href="http://smallestdotnet.com/">check</a> or <a href="https://www.microsoft.com/net/download/framework">download</a></small>
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
            
            <a name="License"></a>
            <div class="panel-heading">                    

            <h5 class="panel-title">Licensing</h5>
            </div>
            <div class="panel-body">
                <p>
                    Although we provide the <a href="https://github.com/RickStrahl/MarkdownMonster">source in the open</a>,
                    Markdown Monster is licensed software.
                </p>


                <p>
                    Markdown Monster can be downloaded and evaluated for free, but <a href="https://store.west-wind.com/product/order/markdown_monster">
                    a reasonably priced license</a> must be purchased for continued use. Licenses are per-user, 
                    rather than per-machine, so a licensed user can use Markdown Monster on as many 
                    computers as desired.
                </p>
                <p>
                    Thanks for playing fair.
                </p>
                <p>

                    <a href="https://store.west-wind.com/product/order/markdown_monster" class="btn btn-lg btn-primary">
                        <i class="fa  fa-credit-card"></i>
                        Purchase
                    </a>
                </p>
                
                <hr />

                <p>                    
                    <h4>License</h4>                        
                    Markdown Monster can be downloaded and evaluated for free, but a license must be purchased for continued
                    use. Licenses are per user, rather than per machine, so a licensed user can use Markdown Monster on as many computers
                    as needed. An organizational license is also available to use Markdown Monster for an unlimited number of installations 
                    within a single organization. Each individual user requires a separate license, unless an organizational license 
                    is used in which case the license is limited to members of licensed organization.                  
                </p>
                
                <p>
                    Licenses are valid for the major version for which it was purchased such as v1.0 to v1.99.                    
                </p>
                
	            <a name="Contribute"></a>
                <h3>Contribute - Get a Free License</h3>
                <p>Contributors that provide valuable feedback with quality bug reports and enhancement requests, 
				   or help out with code via Pull Requests, or support Markdown Monster in a significant 
					way are all eligible for a free license.
				   <br />
                
	            <p>
		            Microsoft employees and Microsoft MVPs as well as employees of any company 
		            offering free tools to the MS MVP program also qualify for a free license.
	            </p>
				<p>   
					<a href="http://west-wind.com/contact/" target="top">Contact Rick for more info</a>
					or - just as likely - I'll be contacting you.
                </p>
                <p>
					
                    <h4>No Warranty</h4>
                    You expressly acknowledge and agree that use of the licensed application is at your sole risk and that
                    the entire risk as to satisfactory quality, performance, accuracy and effort is with you. To the maximum
                    extent permitted by applicable law, the license application and any services performed or provided by
                    the licensed application ("services") are provided "as is" and "as available," with all faults and without
                    warranty of any kind, and application provider hereby disclaims all warranties and conditions with respect
                    to the licensed application and any services, either express, implied or statutory, including, but not
                    limited to, the implied warranties and/or conditions of merchantability, of satisfactory quality, of
                    fitness for a particular purpose, of accuracy, of quiet enjoyment, and non-infringement of third party
                    rights. Application provider does not warrant against interference with your enjoyment of the licensed
                    application, that the functions contained in, or services performed or provided by, the licensed application
                    will meet your requirements, that the operation of the licensed application or services will be uninterrupted
                    or error-free, or that effects in the licensed application or services will be corrected. No oral or
                    written information or advice given by application provider or its authorized representative shall create
                    a warranty. Should the licensed application or services prove defective, you assume the entire cost
                    of all necessary servicing, repair or correction.                     
                </p>

                <p>
                    <h4>LIMITATION OF LIABILITY</h4>
                    IN NO EVENT SHALL THE AUTHOR, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR REDISTRIBUTE THIS PROGRAM AND DOCUMENTATION, BE LIABLE
                    FOR ANY COMMERCIAL, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY
                    TO USE THE PROGRAM INCLUDING, BUT NOT LIMITED TO, LOSS OF DATA OR DATA BEING RENDERED INACCURATE OR
                    LOSSES SUSTAINED BY YOU OR LOSSES SUSTAINED BY THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE
                    WITH ANY OTHER PROGRAMS, EVEN IF YOU OR OTHER PARTIES HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES
                </p>
                


            </div>
            

        </div>
        <div class="clearfix"></div>            
    </div>
        <div style="height: 50px;"></div>
   </div>

    <nav class="banner" style="font-size: 8pt; padding: 10px; height: 80px; border-top: solid black 4px;border-bottom: none;">
        <div class="right">
            created by:<br />
            <a href="http://west-wind.com/" style="padding: 0;">
                <img src="/Images/wwToolbarLogo.png" style="width: 150px;" />
            </a>
        </div>
        &copy; West Wind Technologies, 2000-<%= DateTime.Now.Year %>
    </nav> 
    

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
            _version = "0.79";
            ReleaseDate = "November 4th, 2016";

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
</script>