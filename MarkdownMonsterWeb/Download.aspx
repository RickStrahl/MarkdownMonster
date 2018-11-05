<%@ Page Language="C#" %>
<%@ Register TagPrefix="ww" Namespace="Westwind.Web.MarkdownControl" Assembly="Westwind.Web.MarkdownControl" %>
<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.IO" %>
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
                                                                        
                    </div>
                    <div class=" col-sm-7">
	                    
						<a class="btn btn-lg btn-info" href="https://west-wind.com/files/MarkdownMonsterSetup.exe">
                            <i class="fa fa-download fa-2x" style="font-size: 0.8em; "></i> &nbsp; 
                            Download Markdown Monster
                        </a>
						
	                    <a href="http://www.softpedia.com/get/Office-tools/Text-editors/Markdown-Monster.shtml#status">
		                    <img src="images/Softpedia.png" style="margin: 5px; margin-left: 15px; height: 115px;"/>
	                    </a>
                        
						
                        <div class="small" style="margin-top: 10px">alternates: 
                            <a href="https://west-wind.com/files/MarkdownMonsterSetup.zip" title="Full Setup exe wrapped in a zip file for those that can't download binaries directly.">Setup Zip</a> | 
                            <a href="https://west-wind.com/files/MarkdownMonsterPortable.zip" title="Fully self contained folder structure for Markdown Monster that can run without installation. Adds some limitations: No .md file association, no global command line access and Addins may not install if running out of a non-privileged folder.">Portable Zip</a> | 
                            <a href="https://west-wind.com/files/MarkdownMonsterSetup_Latest.exe" title="Latest pre-release installer that might be slightly ahead of the current release version.">Latest pre-Release</a>
                            <small style="font-size: 0.7em">(<%= LatestVersion %>)</small>
                        </div> 
                        <div style="margin-top: 15px;">
                            <div class="fa fa-info-circle" style="font-size: 280%; color: steelblue; float: left;"></div>
                            <div style="margin-left: 50px;">
                                
                                <div class="small">  
                                    <p>
                                    Markdown Monster can be downloaded and evaluated for free, but a 
                                    <a href="https://store.west-wind.com/product/order/markdown_monster">reasonably priced license</a>
                                    must be purchased for continued use. Licenses are per-user, so you can use 
                                    Markdown Monster on as many computers you wish with your license.
                                    </p>
                                    <p>
                                    Want a <a href="#Contribute">free license</a>? Contributors who help out with code, feature suggestions, 
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
                                Microsoft .NET Framework 4.6.2 or later<small>
                                    <a href="http://smallestdotnet.com/">check</a> or <a href="https://www.microsoft.com/net/download/all">download</a></small>
                            </div>   
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Internet Explorer 11
                            </div>
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Git <small>(optional)</small>
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
                            <div>
                                <i class="fa fa-check-circle"></i>
                                Rock your Markdown like a Pro
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

     <ww:Markdown runat="server" id="license" NormalizeWhiteSpace="true">

Although we provide the <a href="https://github.com/RickStrahl/MarkdownMonster">source in the open</a>, Markdown Monster is licensed software. Markdown Monster can be downloaded and evaluated for free, but [a reasonably priced license](https://store.west-wind.com/product/order/markdown_monster) must be purchased for continued use. 

<a href="#License">More detailed license information can be found below</a>.
                    
Thank you for playing fair.

<a href="https://store.west-wind.com/product/order/markdown_monster" class="btn btn-lg btn-primary">
    <i class="fa  fa-check" style="color: lightgreen"></i>
    Purchase a License
</a>

---
         
<a name="Contribute"></a>
#### Contribute - Get a Free License
Contributors that provide valuable feedback with quality bug reports or enhancement requests, 
help out with code via Pull Requests, help promote Markdown Monster, or support Markdown Monster in
any other significant way are all eligible for a free license.
        
<a href="http://west-wind.com/contact/" target="top">Contact Rick for more info</a>
or - just as likely - I'll be contacting you.
         
#### MVPS, ASP.NET Insiders and Microsoft Employees Get a Free License				   
Microsoft MVPs, ASP.NET Insiders and Microsoft employees as well as employees of any company offering free tools to 
the Microsoft MVP program also qualify for a free license.
	            
<a href="https://store.west-wind.com/mvpperks.aspx" target="top">Apply for free license</a>.
         
--- 
         
<h3 id="License">Markdown Monster License</h3>
         
Markdown Monster comes in several license modes: Evaluation, Single User, Multiple User and Site License.

Markdown Monster is source open with source code available on GitHub, but it is a licensed product that requires a paid-for license for continued use. The software is licensed as © Rick Strahl, West Wind Technologies, 2015-2018. 

A fully functional, free evaluation version is available for evaluation use, but continued use requires purchase of a license. 

Licenses can be purchased from:
http://store.west-wind.com/product/markdown_monster


#### Evaluation License

The evaluation version is unrestricted and has all the features and functionality of the registered version, but shows occasional freeware notices in the user interface. Tampering with or removing of the notice is not allowed with the evaluation license.

Evaluation is allowed for up to 50 uses of running Markdown Monster. After 50 uses you need to purchase a license.

#### Purchased License

For continued used of Markdown Monster a paid-for license is required. The paid-for license removes the freeware notices.

Each licensed user must have a separate license, but a single user may use multiple copies of Markdown Monster on multiple machines, given that only one copy at a time is in use.

The multi-user license works the same as a single user license applied to the number of users specified on the licensed purchased.

An organizational site license is available to allow any number of users running unlimited numbers of Markdown Monster instances within a single organization.

Any purchased license is valid for the duration of the major release that it was purchased for (ie. 1.00-1.99) and updates within the major version are always free. Upgrade pricing is available for major version upgrades and it's our policy to allow for free upgrades to the next major version within a year of purchase.

#### Source Code

The Markdown Monster source code is available on GitHub at https://github.com/RickStrahl/MarkdownMonster, and we allow modification of source code for internal use of Markdown Monster in your organization or for submitting pull requests to the Markdown Monster main repository. Under no circumstances are you allowed to re-package and re-distribute any part of Markdown Monster outside of your organization.

We encourage pull requests for feature suggestions or bug fixes to be submitted back to the Markdown Monster repository. Any contributors that provide meaningful enhancements, help with identifying and or fixing of bugs or by actively promoting Markdown Monster can qualify for a free license (at our discretion). Additionally Microsoft MVPs and Insiders and Microsoft Employees can apply for a free license.

#### WARRANTY DISCLAIMER: NO WARRANTY!

YOU EXPRESSLY ACKNOWLEDGE AND AGREE THAT USE OF THE LICENSED APPLICATION IS AT YOUR SOLE RISK AND THAT THE ENTIRE RISK AS TO SATISFACTORY QUALITY, PERFORMANCE, ACCURACY AND EFFORT IS WITH YOU. TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THE LICENSE APPLICATION AND ANY SERVICES PERFORMED OR PROVIDED BY THE LICENSED APPLICATION ("SERVICES") ARE PROVIDED "AS IS" AND "AS AVAILABLE," WITH ALL FAULTS AND WITHOUT WARRANTY OF ANY KIND, AND APPLICATION PROVIDER HEREBY DISCLAIMS ALL WARRANTIES AND CONDITIONS WITH RESPECT TO THE LICENSED APPLICATION AND ANY SERVICES, EITHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES AND/OR CONDITIONS OF MERCHANTABILITY, OF SATISFACTORY QUALITY, OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY, OF QUIET ENJOYMENT, AND NON-INFRINGEMENT OF THIRD PARTY RIGHTS. APPLICATION PROVIDER DOES NOT WARRANT AGAINST INTERFERENCE WITH YOUR ENJOYMENT OF THE LICENSED APPLICATION, THAT THE FUNCTIONS CONTAINED IN, OR SERVICES PERFORMED OR PROVIDED BY, THE LICENSED APPLICATION WILL MEET YOUR REQUIREMENTS, THAT THE OPERATION OF THE LICENSED APPLICATION OR SERVICES WILL BE UNINTERRUPTED OR ERROR-FREE, OR THAT EFFECTS IN THE LICENSED APPLICATION OR SERVICES WILL BE CORRECTED. NO ORAL OR WRITTEN INFORMATION OR ADVICE GIVEN BY APPLICATION PROVIDER OR ITS AUTHORIZED REPRESENTATIVE SHALL CREATE A WARRANTY. SHOULD THE LICENSED APPLICATION OR SERVICES PROVE DEFECTIVE, YOU ASSUME THE ENTIRE COST OF ALL NECESSARY SERVICING, REPAIR OR CORRECTION.

IN NO EVENT SHALL THE AUTHOR, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR REDISTRIBUTE THIS PROGRAM AND DOCUMENTATION, BE LIABLE FOR ANY COMMERCIAL, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY TO USE THE PROGRAM INCLUDING, BUT NOT LIMITED TO, LOSS OF DATA OR DATA BEING RENDERED INACCURATE OR LOSSES SUSTAINED BY YOU OR LOSSES SUSTAINED BY THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE WITH ANY OTHER PROGRAMS, EVEN IF YOU OR OTHER PARTIES HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
                	
                                
</ww:Markdown>

            </div>
            

        </div>
        <div class="clearfix"></div>      
            
        
    </div>
        
    <nav class="banner banner-bottom" style="font-size: 8pt; padding: 10px; height: 80px; border-top: solid black 4px;border-bottom: none;">
        <div class="right">
            created by:<br />
            <a href="http://west-wind.com/" style="padding: 0;">
                <img src="Images/wwToolbarLogo.png" style="width: 150px;" />
            </a>
        </div>
        &copy; West Wind Technologies, 2015-<%= DateTime.Now.Year %>
    </nav> 
   </div>

   
    
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