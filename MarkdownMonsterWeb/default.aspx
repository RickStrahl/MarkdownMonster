<%@ Page Language="C#" %>

<%@ Import Namespace="System.Diagnostics" %>
<%@ Import Namespace="System.Net" %>

<!DOCTYPE html>
<html>
<head>
    <title>Markdown Monster - A better Markdown Editor for Windows</title>
    
    <meta name="description" content="Markdown Monster: An easy to use Markdown Editor and Weblog Publishing tool for Windows. Create Markdown with a low key interface that gets out of your way, but provides advanced features to help you be more productive." />
    <meta name="keywords" content="markdown, windows, markdown editor,text editor,documentation,weblog,publishing,screen capture,writing,documentation,open source,extensible,addins,wpf,dotnet,westwind,rick strahl" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    
    <meta name="company" content="West Wind Technologies - https://west-wind.com"/>
    <meta name="author" content="Rick Strahl, West Wind Technologies  - https://weblog.west-wind.com"/>


    <link href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.css" rel="stylesheet" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css" rel="stylesheet" />

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
        #Carousel .carousel-inner > img { height: 845px; }
        #Carousel2 .carousel-inner > .item {
            margin: 5px 11%;
        }
        #ShareBox {
                    flex-direction: row;
                    padding: 0 20px;
                    height: 50px;
                    background: #252525;

                    display: flex;
                    flex-direction: row;                    
                }
                .share-buttons {
                    display: inline;
                    font-size: 35px;
                    padding: 0;                    
                }
                .share-buttons li {
                    display: inline;
                    list-style: none;
                    padding-right: 20px;
                }
                    .share-buttons a {
                        text-decoration: none;
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

    <meta name="twitter:card" content="https://markdownmonster.west-wind.com/Images/MarkdownMonsterMonsterBigger.png">
    <meta name="twitter:site" content="@markdownmonstr">
    <meta name="twitter:title" content="Markdown Monster - a better Markdown Editor and Weblog Publisher for Windows">
    <meta name="twitter:description" content="Markdown Monster is an interactive Markdown Editor dedicated to optimizing Markdown content creation with a capable sytnax highlighted editor, live preview, inline spell-checking and many out of your way helpers that facilitate embedding images, links, code, tables, emojiis and more.">
    <meta name="twitter:creator" content="@markdownmonstr">
    <meta name="twitter:image" content="https://markdownmonster.west-wind.com/Images/MarkdownMonsterMonsterBigger.png">
    <meta name="twitter:domain" content="markdownmonster.west-wind.com">
</head>
<body>
    <div class="banner">
        <span class="banner-title" style="cursor: pointer;" onclick="window.location = './';">
            <img src="Images/MarkdownMonster_Icon_32.png"
                style="height: 28px;"
                alt="Markdown Monster" />
            <span class="hidable-xs">Markdown Monster</span>
        </span>
        <nav id="TopMenu" class="right">
            <a href="./" class="active">
                Home
            </a>
            <a href="https://www.youtube.com/watch?v=XjFf57Ap9VE">                
                Video
            </a>
            <%--<a href="features.aspx">Features</a>--%>
            <a href="download.aspx" class="hidable" title="Download Markdown Monster">                
                Download
            </a>
            <a href="purchase.aspx" title="Purchase Markdown Monster">                
                Buy
            </a>                 
            <a href="https://support.west-wind.com/Thread4NM0M17RC.wwt" class="hidable" title="Support for Markdown Monster">                
                Support
            </a>
            <a href="docs/" title="Markdown Monster Documentation">                
                Docs
            </a>            
            <a href="https://medium.com/markdown-monster-blog/" title="Markdown Monster Weblog">                
                Blog
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
                            
          
                            <br />
                            <small style="font-size: 8pt;"><i>version <%= Version %> - <%= ReleaseDate%></i></small>
                        </nav>
                        
                        

                        <h1 style="font-weight: bold; display: none">Markdown Monster</h1>
                        
                    </div>
    
                    <div class="top-bullet-box">
                        <h4 style="color: #ffd281;">Markdown Editing and Weblog Publishing on Windows</h4>
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
                                        Embed images, links and emoji
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Paste images from Clipboard
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Capture &amp; embed screen shots
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Save to Html and Pdf
                                    </div>                              
                                </div>
                            </div>
                            <div class="col-md-6">

                                <div class="bullet-box-items">                                    
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Weblog publishing and editing
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
                                        Vim and Emacs support
                                    </div>  
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Pandoc rendering &amp; conversion
                                    </div>
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        Template text expansion
                                    </div>   
                                    <div>
                                        <i class="fa fa-check" style="color: lightgreen"></i>
                                        .NET based scripting & addins
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                       
                    </div>

                     
                            
            <div id="ShareBox">
                    <div style="flex: none; align-self: center;margin-right: 20px;">Share on:</div>    
                    <div class="pull-left" style="flex: none">  
                    
                    <ul class="share-buttons" style="flex: none">
                         <li>
                            <a href="https://twitter.com/intent/tweet?source=https://markdownmonster.west-wind.com/&amp;text=Check out Markdown Monster - A better better Markdown Editor and Weblog Publisher for Windows: https://markdownmonster.west-wind.com" target="_blank" title="Tweet">
                                <i class="fa fa-twitter-square"></i>
                            </a>
                        </li>
                        <li>
                            <a href="https://www.facebook.com/sharer/sharer.php?u=https://markdownmonster.west-wind.com/&amp;t=Markdown Monster - A better better Markdown Editor and Weblog Publisher for Windows" target="_blank" title="Share on Facebook">
                                <i class="fa fa-facebook-square"></i>
                            </a>
                        </li>                       
                        <li>
                            <a href="http://www.reddit.com/submit?url=https://markdownmonster.west-wind.com/&amp;title=Markdown Monster - A better better Markdown Editor and Weblog Publisher for Windows" target="_blank" title="Submit to Reddit">
                                <i class="fa fa-reddit-square"></i>
                            </a>
                        </li>
                    </ul>
                </div>
                <div class="right created-by" style="margin-bottom: 2px; flex: 1 1 auto;align-self: flex-end">
                    <div style="font-size: xx-small">created by:</div>
                    <a href="http://west-wind.com">
                        <img src="/Images/wwToolbarLogo.png" 
                            style="height: 30px" alt="West Wind Technologies" />
                    </a>
                </div>
            </div>

                
</div>
            </header>

            <article class="content" >
           

            <h3 style="clear: both">Better Markdown Editing for Windows</h3>
            <p>
                Markdown is everywhere these days and it's used for all sorts
                of different purposes. Wouldn't it be nice if you have an editor that can keep 
                up with <strong>all</strong> of those scenarios?
            </p>
                
            <p>                    
                Markdown Monster is a Markdown editor and viewer that lets you edit Markdown with syntax highlighting
                and fast text entry. A collapsible, synced, live preview lets you see your output as you type.
                You can easily embed images, links, emojis and code as text or by using our gentle UI
                helpers that simplify many operations. You can also paste and drag images directly into the editor.
                Inline spell-checking and word counts keep your content streamlined unobtrusively.
            </p>
                
                
            <p>
                You can export Markdown to HTML by saving to disk or by copying Markdown selections as
                HTML directly to the clipboard. The HTML preview can display syntax colored code snippets 
                for most common coding languages, and can easily be customized with HTML and CSS template to match 
                your own sites. You can choose from a light and dark theme, and choose individual editor and  
                preview themes. You can even use Vim or EMacs type conventions. Other convenience features let you browse for and 
                select files in the built-in folder browser, jump to the current folder in Explorer or Terminal,
                commit to Git and more.
            </p>
                
                <div class="content" style="padding: 0 0 20px">
                    <%-- <h2>Ready to get started?</h2>
                <p>
                    Here are a few screen shots and a video to give you an idea what to expect when 
                    you run Markdown Monster. The user interface is
                    simple and easy to use, and that's the way it should be
                    to make it quick and easy to test your Web sites.
                </p>--%>
                    <div id="Carousel" class="carousel slide" data-ride="carousel" style="margin-right: -7%; margin-left: -7%">
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
                                <img src="Images/screenshot_light.png" />
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
                                <img src="Images/ScreenCapture.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/AddinManager.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
							
	                        <div class="item">
		                        <img src="Images/FolderBrowser.png" />
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
                

             

                <h4>Weblog Publishing</h4>
                <p>
                    Markdown Monster can also publish your Markdown directly to your Weblog. If your blog supports 
					WordPress, MetaWeblog or Medium, you can publish your documents with one click. You can also 
					edit and republish, or download existing posts and even convert existing posts from HTML to Markdown.
                 </p>
                
                <h4>Extensible via .NET Addins</h4>
                 <p>
                    We also wanted to make sure <b>the editor is highly extensible</b>, so you can add custom features
                    of your own. Markdown Monster includes an addin model that makes it easy to build
                    extensions that let you hook into the UI, the editor behavior and the publishing
                    process. We also provide useful .NET Scripting and Text Templating addins that
					let you automate many tasks without creating an addin. Find out more about 
                    <a href="http://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm" target="top">
                    creating an addin with .NET</a>.
                </p>                

                    <h4>What our Users say</h4>
                    <p>
                    We work hard at building an editor that you love to use, and that provides you with the features you need.
                    Your <a href="https://github.com/rickstrahl/MarkdownMonster/issues">feedback matters</a> and we'd love to
                    hear your suggestions and see you get involved. 
                    </p>
                    <p style="margin-bottom: 14px;">But don't take our word for it - here is what some of 
                    our users are saying about Markdown Monster:
                    </p>

                    <!-- Tweets  -->  
                    <div id="Carousel2" class="carousel slide" data-ride="carousel" style="margin-right: -7%; margin-left: -7%">
                        <!-- Indicators -->
                        <ol class="carousel-indicators">
                            <li data-target="#Carousel" data-slide-to="0" class="active"></li>
                        </ol>

                        <!-- Wrapper for slides -->
                        <div class="carousel-inner" >
                              
                            <div class="item active">
                                <img src="Images/Tweets/JeremyMorgan.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>


                            <div class="item">
                                <img src="Images/Tweets/angrypets.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>

                            <div class="item">
                                <img src="Images/Tweets/atlantabass.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/Tweets/Cook.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/Tweets/honeycutt.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/Tweets/Anaerin.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>

							<div class="item">
                                <img src="Images/Tweets/sstranger.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/Tweets/DougHennig.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            <div class="item">
                                <img src="Images/Tweets/benb.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>

                            <div class="item">
                                <img src="Images/Tweets/_bron_.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>
                            
                            
                            <div class="item">
                                <img src="Images/Tweets/James_Willock2.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>	
                            
                            
                            <div class="item">
                                <img src="Images/Tweets/dbVaughan.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>

                            <div class="item">
                                <img src="Images/Tweets/ArthurZ.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>	
                            
                            <div class="item">
                                <img src="Images/Tweets/vmoench.png" />
                                <div class="carousel-caption">
                                </div>
                            </div>	
                  
                        
                        </div>

                        <!-- Controls -->
                        <a class="left carousel-control" href="#Carousel2" data-slide="prev">
                            <span class="glyphicon glyphicon-chevron-left"></span>
                        </a>
                        <a class="right carousel-control" href="#Carousel2" data-slide="next">
                            <span class="glyphicon glyphicon-chevron-right"></span>
                        </a>
                    </div>
                    
                    <small>Want to share your excitement for Markdown Monster? Tweet to <a href="https://twitter.com/markdownmonstr">@markdownmonstr</a> and tell us how Markdown Monster improves your world.</small>

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
                                        File and Folder Browser
                                    </div>

                                    <div>
                                        <i class="fa fa-check-circle"></i> 
                                        Easy image and link embedding
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i> 
                                        Paste images from Clipboard
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Built in screen capture
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Two-way Table Editor
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
                                        Dark and Light UI Themes
                                    </div>                                    
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Editor and Preview Theming
                                    </div>                                    
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Weblog publishing and editing
                                    </div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Yaml Meta Data Support
                                    </div>                                                                      
									<div>
										<i class="fa fa-check-circle"></i>
										Integrated Folder Browser
									</div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Expandable text templates
                                    </div>   
									<div>
										<i class="fa fa-check-circle"></i>
										Swappable Markdown Parsers (Markdig,Pandoc)
									</div>
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        Extensible with .NET Add-ins
                                    </div>                   
                                    <div>
                                        <i class="fa fa-check-circle"></i>
                                        High DPI Support
                                    </div>                   
                                </div>
                            </div>
                        </div>

                        <div class="col-sm-6" >
                            <div class="panel panel-default">
                                <div class="panel-heading">
                                    <h5 class="panel-title">Get it</h5>
                                </div>
                                <div class="panel-body">
                                    <p>
                                    You can simply download and install Markdown Monster from
                                    our Web site.
                                    </p>
                                    <div style="margin: 21px 10px;">
                                        <a href="download.aspx" style="display: block; margin-bottom: 15px;">
                                            <img src="images/download.gif" class="boxshadow roundbox">
                                        </a>
                                    </div>
                                    
                                    <div style="margin-top: 15px">                                        
                                        <p>
                                            Alternately you can also install Markdown Monster 
                                    using the Chocolatey installer:
                                        </p>
                                        <div style="margin: 20px 10px;">
                                            <a href="http://chocolatey.org/packages/MarkdownMonster" style="display: block">
                                                <img src="images/chocolatey.png" style="width: 170px;margin-bottom: 8px; display: block;" alt="Chocolatey" />
                                            </a>

                                            <pre style="font-size: 10pt; font-family: Consolas, monospace; color: whitesmoke; background: #535353">c:\> choco install markdownmonster</pre>
                                            
                                            <p>or use the <a href="https://chocolatey.org/packages/MarkdownMonster.Portable">portable, non-admin installer</a>:</p>

                                            <pre style="font-size: 10pt; font-family: Consolas, monospace; color: whitesmoke; background: #535353">c:\> choco install markdownmonster.portable</pre>
                                        </div>
                                    </div>
                                 
                                    <p>
                                        <i class="fa fa-newspaper-o" style="color: gold"></i>
                                        <a href="https://github.com/RickStrahl/MarkdownMonster/blob/master/Changelog.md">
                                            
                                            Check out what's new
                                        </a>
                                    </p>

                                    <b>Requirements
                                    </b>
                                    <p>
                                        <ul>
                                            <li>Windows 7 or newer, 2008 or newer</li>
                                            <li>.NET Framework 4.6 or later<br />
                                                (<a href="http://smallestdotnet.com">check</a> or <a href="https://www.microsoft.com/net/download/framework">download</a>)</li">
                                            <li>Internet Explorer 11</li>
                                        </ul>
                                   
                                    <div style="margin-top: 10px">                                        
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
        
        <nav class="banner banner-bottom" >
            <div class="right">
                created by:<br />
                <a href="http://west-wind.com/" style="padding: 0;">
                    <img src="Images/wwToolbarLogo.png" style="width: 150px;" />
                </a>
            </div>
            &copy; West Wind Technologies, <%= DateTime.Now.Year %>
        </nav>
    </div>

    

    <script src="//code.jquery.com/jquery-1.11.0.min.js"></script>
    <script src="Css/js/bootstrap.min.js"></script>
    <script src="Css/js/touchswipe.js"></script>
    <script>
        $(document).ready(function () {

            $("#Carousel").carousel({
                interval: false
            });


            $("#Carousel2").carousel({
                interval: 8000
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

            $(".carousel-inner img").click(function() {
                window.open(this.src);
            });
        });
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
