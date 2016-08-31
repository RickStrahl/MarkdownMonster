<%@ Page Language="C#" %>

<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.Net" %>

<!DOCTYPE html>
<html>
<head>
    <title>Markdown Monster</title>
    <meta name="description" content="Markdown Monster is an easy to use and extensible Markdown Editor and Weblog Publisher for Windows. Easily edit Markdown documents, convert HTML to markdown and publish Markdown to your Weblog." />
    <meta name="keywords" content="Markdown,Editor,Editing,Weblog,Publish,Writing,Documentation" />

    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.min.css" rel="stylesheet" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet" />

    <link rel="shortcut icon" href="favicon.ico" />
    <link rel="icon" type="image/png" href="favicon.png" />
    <meta content="Images/MarkdownMonster_Icon_128.png" itemprop="image">
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
            <a href="./" class="active">Home</a>
            <%--<a href="https://youtu.be/O5J8mDfVZH8">Video</a>--%>
            <%--<a href="features.aspx">Features</a>--%>
            <a href="download.aspx">Download</a>
            <a href="http://store.west-wind.com/product/markdown_monster">Buy</a>
            <a href="docs/">Docs</a>
            <%--<a href="pricing.aspx">License</a>--%>
        </div>
    </div>
    
    <div id="MainContainer" class="background">
        <div id="ContentContainer" class="content">
            <header style="background-color: #535353; color: whitesmoke;">
                <div style="background: black; padding: 0 0;">
                    <div style="width: 100%;margin: 0 4%;">
                        <img src="Images/MarkdownMonsterMonsterBigger.png" alt="Markdown Monster"  />
                        
                        <style>
                        </style>
                         <div id="ActionButtons" style="width: 100%; margin: 20px auto 40px; text-align: center;">
                            
                            <a href="download.aspx" class="btn btn-lg btn-success" style="background: #222" >
                                <i class="fa fa-download"></i>
                                <b style="color: cornsilk">Free Download</b>
                            </a>
                            <a href="http://github.com/rickstrahl/MarkdownMonster" class="btn btn-lg btn-success" style="background: #222" >
                                <i class="fa fa-github"></i>
                                GitHub
                            </a>
                            
                            <%-- <a href="http://chocolatey.org/WestwindMarkdownMonster" class="btn btn-lg btn-success" style="background: #222" >
                                <i class="fa  fa-cloud-download"></i>
                                 <i class="fa fa-"></i>
                                 Chocolatey
                            </a>--%>
                           
                            <br />
                            <small style="font-size: 8pt;"><i>version <%= Version %> rc2 - <%= ReleaseDate%></i></small>
                        </div>
                        
                        

                        <h1 style="font-weight: bold; display: none">Markdown Monster</h1>
                        
                    </div>

                    <div class="top-bullet-box">
                        <h4 style="color: #ffd281;">Extensible Markdown Editing and Weblog Publishing for Windows</h4>
                        <div class="row">
                            <div class="col-md-6">
                                <div style="padding: 5px 20px; font-size: 1.3em">
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Syntax colored Markdown
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Live Preview
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Inline Spell Checking
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Easy image and link embedding
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Screen Captures (with SnagIt)
                                    </div>                                    
                                </div>
                            </div>
                            <div class="col-md-6">

                                <div style="padding: 5px 20px; font-size: 1.3em">
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Gentle Toolbar support
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        HTML to Markdown Conversion
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Editor and Preview Themes
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Weblog Post Publishing
                                    </div>                                    
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        .NET based Add-ins
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        
                       
                    </div>
                </div>
            </header>

            
            <div class="content" style="padding: 10px 40px 0; margin: 0">
                <div class="right" style="margin-right: 15px; margin-top: -5px; margin-right: -20px;">
                    <div style="font-size: xx-small">created by:</div>
                    <a href="http://west-wind.com">
                        <img src="/Images/wwToolbarLogo.png" style="float: right; height: 30px" alt="West Wind Technologies" />
                    </a>
                </div>

                <h2>Easy and Extensible Markdown Editing</h2>
                <p>
                    Markdown is everywhere these days, and many of us are using Markdown for all sorts
                    of different purposes. Wouldn't it be nice if you have an editor that can keep 
                    up with <strong>all</strong> of those scenarios?
                </p>
                <p>                    
                    Markdown Monster provides 
                    the editing features you&#39;d expect of a Markdown editor: 
                    You get a responsive text editor that's got you covered with Markdown syntax highlighting, so it it's easy to navigate your Markdown text, an optional collapsible live preview, so you can see what your output looks like, in-line spellchecking and a handful of optimized menu options that help you embed and link content into your markdown. You can easily markup code snippets using many common syntax formats which is also reflected in the preview.<p>                    
                    But we also wanted to build
                    an editor that can be extended to meet the needs of those that are using Markdown in interesting ways to share information. Whether it&#39;s for writing blog content in Markdown or providing a custom front end to an online application that receives Markdown content via a REST API. The main reason for Markdown Monster is that it can be extended while accessing the entire Markdown document and UI which allow you to extend Markdown Monster with your custom functionality. Want to add a custom image capture? Embed images from a photo service? You can do that easily. Want to publish
                    your Markdown output to your Weblog? You can do that (and we provide an addin for
                    just that with the base installation). Using Markdown Monster's add-in interface 
                    you you can integrate and access the editor to inject markdown text, control the UI
                    or bring up an entirely separate interface (using .NET and WPF) to access the current 
                    document and interact with it. The sky's the limit. Find out more about 
                    <a href="http://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm" target="top">
                        creating an addin with .NET</a>.
                </p>
                <p>
                    The released version ships with a couple of useful addins that provide embeddable
                    screen captures using the popular SnagIt tool from Techsmith, and a Weblog publishing 
                    interface that lets you publish your Markdown content
                    to Wordpress or MetaWebLogApi based blog engines.
                </p>
                

                <div class="content" style="padding: 0 0 20px">
                    <%-- <h2>Ready to get started?</h2>
                <p>
                    Here are a few screen shots and a video to give you an idea what to expect when 
                    you run Markdown Monster. The user interface is
                    simple and easy to use, and that's the way it should be
                    to make it quick and easy to test your Web sites.
                </p>--%>
                    <div id="Carousel" class="carousel slide" data-ride="carousel">
                        <!-- Indicators -->
                        <ol class="carousel-indicators">
                            <li data-target="#Carousel" data-slide-to="0" class="active"></li>
                        </ol>

                        <!-- Wrapper for slides -->
                        <div class="carousel-inner">
                            
                            <div class="item active">
                                <img src="Images/screenshot.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>

							<div class="item">
                                <img src="Images/WeblogPublishingAddin.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                        
                        </div>

                        <!-- Controls -->
                        <a class="left carousel-control" href="#Carousel" data-slide="prev">
                            <span class="glyphicon glyphicon-chevron-left"></span>
                        </a>
                        <a class="right carousel-control" href="#Carousel" data-slide="next">
                            <span class="glyphicon glyphicon-chevron-right"></span>
                        </a>
                    </div>
                </div>

                <article class="content" style="padding: 10px 0;">
                    <div class="row">
                        <div class="col-sm-6">
                            <div class="panel panel-default">
                                <div class="panel-heading">
                                    <h5 class="panel-title">Features</h5>
                                </div>
                                <div id="FeatureList" class="panel-body">
                                    <div><i class="fa fa-check-circle"></i>
                                        Easy Markdown Editing
                                    </div>
                                    <div><i class="fa fa-check-circle"></i>
                                        Syntax colored Markdown text
                                    </div>
                                    <div><i class="fa fa-check-circle"></i>
                                        Live Preview while you type
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Inline spell checking
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Gentle Toolbar support
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i> 
                                        Easy image and link embedding
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Screen Captures with SnagIt
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Html to Markdown Conversion
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Many Editor Themes
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Easily customizable Preview Themes
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Weblog Publishing
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Html Editing with Live Preview
                                    </div>    
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Extensible with .NET Add-ins
                                    </div>                                    
                                </div>
                            </div>
                        </div>

                        <div class="col-sm-6">
                            <div class="panel panel-default">
                                <div class="panel-heading">
                                    <h5 class="panel-title">Get it</h5>
                                </div>
                                <div class="panel-body">
                                    <p>
                                        You can simply download and install Markdown Monster from
                                    our Web site. Unzip the distribution file and run
                                    the installer.
                                    </p>
                                    <div style="margin: 10px;">
                                        <a href="download.aspx" style="display: block; margin-bottom: 15px;">
                                            <img src="/images/download.gif" class="boxshadow roundbox">
                                        </a>
                                    </div>
                                    
                                    <div style="margin-top: 15px">                                        
                                        <p>
                                            Alternately you can also install Markdown Monster 
                                    using the Chocolatey NuGet installer:
                                        </p>
                                        <div style="margin: 10px;">
                                            <a href="http://chocolatey.org/packages/MarkdownMonster" style="display: block">
                                                <img src="images/chocolatey.png" style="width: 170px;" alt="Chocolatey" />
                                            </a>

                                            <pre style="font-size: 10pt; font-family: Consolas, monospace; color: whitesmoke; background: #535353">c:\> choco install MarkdownMonster</pre>
                                        </div>
                                    </div>
                                 


                                    <b>Requirements
                                    </b>
                                    <p>
                                        <ul>
                                            <li>Windows Vista or newer, 2008 or newer</li>
                                            <li>.NET Framework 4.5 or later</li>
                                        </ul>
                                    </p>
                                    
                                      <div style="margin-top: 15px">                                        
                                       <a style="font-size: 1.4em" href="http://twitter.com/MarkdownMonstr"><i class="fa fa-twitter"></i>
                                           Follow us on Twitter
                                       </a>
                                    </div>

                                </div>
                                   
                                   
                            </div>
                        </div>




                    </div>



                </article>
            </div>
            <div class="clearfix"></div>
        </div>
    </div>

    <nav class="banner" style="font-size: 8pt; padding: 10px; height: 80px; margin: 0; border-top: solid black 4px; border-bottom: none;">
        <div class="right">
            created by:<br />
            <a href="http://west-wind.com/" style="padding: 0;">
                <img src="/Images/wwToolbarLogo.png" style="width: 150px;" />
            </a>
        </div>
        &copy; West Wind Technologies, <%= DateTime.Now.Year %>
    </nav>

    <script src="//code.jquery.com/jquery-1.11.0.min.js"></script>
    <script src="Css/js/bootstrap.min.js"></script>
    <script src="Css/js/touchswipe.js"></script>
    <script>
        $(document).ready(function () {

            $("#Carousel").carousel({
                interval: false
            });

            //Enable swiping...
            $(".carousel-inner").swipe({
                //Generic swipe handler for all directions
                swipeRight: function (event, direction, distance, duration, fingerCount) {
                    $(this).parent().carousel('prev');
                },
                swipeLeft: function () {
                    $(this).parent().carousel('next');
                },
                //Default is 75px, set to 0 for demo so any distance triggers swipe
                threshold: 0
            });
        });
    </script>

    <script>
      (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
      (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
      m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
      })(window,document,'script','https://www.google-analytics.com/analytics.js','ga');
      ga('create', 'UA-9492219-14', 'auto');
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
            _version = "0.90";
            ReleaseDate = "July 7th, 2015";

            try
            {
                WebClient client = new WebClient();
                string xml = client.DownloadString("http://west-wind.com/files/markdownmonster_version.xml");
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
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write(ex.Message);
            }

            return _version;
        }
    }
    private static string _version;
    public static string ReleaseDate;
</script>
