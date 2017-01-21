<%@ Page Language="C#" %>

<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.Net" %>

<!DOCTYPE html>
<html>
<head>
    <title>Markdown Monster - A better Markdown Editor for Windows</title>
    <meta name="description" content="Markdown Monster is an easy to use Markdown Editor and Weblog Publisher for Windows." />
    <meta name="keywords" content="markdown, text editor, documentation, editor, windows, weblog, publishing, screen capture, writing, open source, extensible, addins" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    
    <meta name="company" content="West Wind Technologies - http://west-wind.com"/>
    <meta name="author" content="Rick Strahl, West Wind Technologies  - http://weblog.west-wind.com"/>

    <link href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.css" rel="stylesheet" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet" />

    <link rel="shortcut icon" href="favicon.ico" />
    <link rel="icon" type="image/png" href="favicon.png" />
    <meta content="favicon.png" itemprop="image">
    
    <style>
        #ActionButtons {
            width: 100%;
            margin: 20px auto 40px;
            text-align: center;
        }
            #ActionButtons a {
                background: #333;
                background: linear-gradient(to bottom, #393939, #222);
            }
    </style>
    
        
    <meta property="og:type" content="website">
    <meta property="og:site_name" content="Markdown Monster">

    <meta property="og:title" content="Markdown Monster: A better Markdown Editor for Windows">
    <meta property="og:description" content="Markdown Monster is an easy to use Markdown Editor and Weblog Publisher for Windows.">
    <meta property="og:url" content="https://markdownmonster.west-wind.com">
    
    <meta property="og:image" content="https://markdownmonster.west-wind.com/Images/MarkdownMonsterMonsterBigger.png">
    <meta property="og:image:type" content="image/png">
    <meta property="og:image:width" content="1024">
    <meta property="og:image:height" content="639">
</head>
<body>
    <div class="banner">
        <span class="banner-title hidable" style="cursor: pointer;" onclick="window.location = './';">
            <img src="Images/MarkdownMonster_Icon_32.png"
                style="height: 28px;"
                alt="Markdown Monster" />
            <span class="hidable-xs">West Wind</span> <span>Markdown Monster</span>
        </span>
        <nav id="TopMenu" class="right">
            <a href="./" class="active">
                Home
            </a>
            <a href="https://www.youtube.com/watch?v=XjFf57Ap9VE">                
                Video
            </a>
            <%--<a href="features.aspx">Features</a>--%>
            <a href="download.aspx" class="hidable-xs">                
                Download
            </a>
            <a href="http://store.west-wind.com/product/markdown_monster">                
                Buy
            </a>                 
            <a href="https://support.west-wind.com/Thread4NM0M17RC.wwt" class="hidable">                
                Support
            </a>
            <a href="docs/">                
                Docs
            </a>
            <%--<a href="pricing.aspx">License</a>--%>
        </nav>
    </div>
    
    <div id="MainContainer" class="background" >
        
       <div class="flex-container" style="display: flex; flex-direction: row;">               
       <div id="ContentContainer" style="flex: 1 1 auto;">
            <header style="background-color: #535353; color: whitesmoke;">
                               
                
                <div style="background: black; padding: 0 0;position: relative">
                    <div style="width: 100%;margin: 0 4%;">
                        <img src="Images/MarkdownMonsterMonsterBigger.png" alt="Markdown Monster"  />
                          <a href="https://github.com/rickstrahl/MarkdownMonster">
                     <img style="position: absolute; top:0; right: 0; border: 0;" src="https://camo.githubusercontent.com/652c5b9acfaddf3a9c326fa6bde407b87f7be0f4/68747470733a2f2f73332e616d617a6f6e6177732e636f6d2f6769746875622f726962626f6e732f666f726b6d655f72696768745f6f72616e67655f6666373630302e706e67" alt="Fork me on GitHub" data-canonical-src="https://s3.amazonaws.com/github/ribbons/forkme_right_orange_ff7600.png">
                 </a>
                        
                        
                         <nav id="ActionButtons" >                            
                            <a href="download.aspx" class="btn btn-lg btn-success"  >
                                <i class="fa fa-download"></i>
                                <b style="color: cornsilk">Free Download</b>
                            </a>
                            <a href="http://github.com/rickstrahl/MarkdownMonster" class="btn btn-lg btn-success">
                                <i class="fa fa-github"></i>
                                GitHub
                            </a>
                       
                            <a href="https://www.youtube.com/watch?v=XjFf57Ap9VE" class="btn btn-lg btn-success"
                               >
                                <i class="fa fa-youtube"></i>
                                Video
                            </a>
                           
                             
                            
                            <%-- <a href="http://chocolatey.org/WestwindMarkdownMonster" class="btn btn-lg btn-success" style="background: #222" >
                                <i class="fa  fa-cloud-download"></i>
                                 <i class="fa fa-"></i>
                                 Chocolatey
                            </a>--%>
                           
                            <br />
                            <small style="font-size: 8pt;"><i>version <%= Version %> - <%= ReleaseDate%></i></small>
                        </nav>
                        
                        

                        <h1 style="font-weight: bold; display: none">Markdown Monster</h1>
                        
                    </div>

                    <div class="top-bullet-box">
                        <h4 style="color: #ffd281;">Extensible Markdown Editing and Weblog Publishing</h4>
                        <div class="row">
                            <div class="col-md-6">
                                <div class="bullet-box-items">
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Syntax colored Markdown
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Live HTML preview
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Inline spell checking
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Image & link embedding
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Capture &amp; embed screen shots
                                    </div>                                    
                                </div>
                            </div>
                            <div class="col-md-6">

                                <div class="bullet-box-items">
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Gentle toolbar support
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        HTML to Markdown conversion
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Editor and preview themes
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Weblog publishing and editing
                                    </div>                                    
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Extensible with .NET add-ins
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        
                       
                    </div>
                </div>
            </header>

            
            <article class="content" >
                <div class="right created-by">
                    <div style="font-size: xx-small">created by:</div>
                    <a href="http://west-wind.com">
                        <img src="/Images/wwToolbarLogo.png" 
                            style="height: 30px" alt="West Wind Technologies" />
                    </a>
                </div>

                <h2>Easy and Extensible Markdown Editing</h2>
                <p>
                    Markdown is everywhere and many of us are using Markdown for all sorts
                    of different purposes. Wouldn't it be nice if you have an editor that can keep 
                    up with <strong>all</strong> of those scenarios?
                </p>
                <p>                    
                    Markdown Monster gives a responsive text editor that has you covered with Markdown syntax highlighting,
                    and fast text entry, so it it's easy to navigate your Markdown text. A collapsable
                    live preview lets you preview your markdown, or export your markdown to HTML. 
                    Inline spell-checking and
                    word counts keep your content streamlined and a handful of optional  toolbar and
                    menu options help you embed and link images and links into your Markdown. Our preview
                    can display syntax colored code snippets for most common coding languages and the
                    preview can be easily customized with HTML and CSS to match your own sites
                    and preferences. Finally, if you're a blogger you can easily take your Markdown and
                    publish it straight to your blog from the editor.
                 </p>
                 <p>
                    In addition to building an attractive and highly functional Markdown editor and viewer, we
                    also wanted to make sure <b>the editor is extensible</b> so you can plug in custom features
                    of your own. Markdown Monster's .NET based add-in API makes it easy to build
                    extensions that let you hook into the UI, the editor behavior and the publishing
                    process. We use this same add-in API to build some of Markdown Monster's internal features 
                    like the Weblog Publisher and Screen Capture. The sky's the limit. Find out more about 
                    <a href="http://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm" target="top">
                    creating an addin with .NET</a>.
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
                                <img src="Images/CodeSnippetInEditor.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>

							<div class="item">
                                <img src="Images/WeblogPublishingAddin.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/WeblogPublishingAddin_Download.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/SnagItCaptureAddin.png" />
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
                
                

                <div class="content" style="padding: 10px 0; flex: 1 1 auto">
                    <div class="row">
                        <div class="col-sm-6">
                            <div class="panel panel-default">
                                <div class="panel-heading">
                                    <h5 class="panel-title">Features</h5>
                                </div>
                                <div id="FeatureList" class="panel-body">
                                    <div><i class="fa fa-check-circle"></i>
                                        Easy and fast Markdown editing
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
                                        Gentle toolbar support
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i> 
                                        Easy image and link embedding
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i> 
                                        Paste and save images
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Screen capture embedding 
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Import Html into Markdown
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Export Markdown to Html
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Many editor themes
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Customizable preview themes
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Weblog publishing and editing
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
                                    our Web site.
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
                                                <img src="images/chocolatey.png" style="width: 170px;margin-bottom: 5px;" alt="Chocolatey" />
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
                                            <li>Internet Explorer 11 or 10</li>
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


                    
                </div>
     
               
            </article>
            <div class="clearfix"></div>
        </div>
        <div class="twitter-sidebar">                         
<%--            <a class="twitter-timeline" data-theme="dark" style="width: 500px;"
               href="https://twitter.com/MarkdownMonstr">Tweets by RickStrahl</a> 
            <script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>     --%>
            
          
            <a class="twitter-timeline" data-chrome="noscrollbar"  data-theme="dark"  data-height="2000" href="https://twitter.com/MarkdownMonstr" >Tweets about @MarkdownMonstr</a>
                                  
           <script async src="//platform.twitter.com/widgets.js" charset="utf-8"></script>         
          
           <%--<a class="twitter-timeline" data-theme="dark" data-chrome="noscrollbar" href="https://twitter.com/search?q=@MarkdownMonstr">Tweets by MarkdownMonstr</a>  --%>
           
        </div> 
        </div><!-- flex -->
    </div>

    <nav class="banner" style="font-size: 8pt; padding: 10px; height: 80px; margin: 0; border-top: solid black 4px; border-bottom: none;">
        <div class="right">
            created by:<br />
            <a href="http://west-wind.com/" style="padding: 0;">
                <img src="/Images/wwToolbarLogo.png" style="width: 350px;" />
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

            if (_version != null && DateTime.UtcNow.Subtract(_lastAccess).TotalMinutes < 10)
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
                _lastAccess = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write(ex.Message);
            }

            return _version;
        }
    }
    private static string _version;
    private static DateTime _lastAccess = DateTime.UtcNow;
    public static string ReleaseDate;
</script>
