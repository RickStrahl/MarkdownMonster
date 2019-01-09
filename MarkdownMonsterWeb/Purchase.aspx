<%@ Page Language="C#" %>
<%@ Register TagPrefix="ww" Namespace="Westwind.Web.Markdown" Assembly="Westwind.Web.Markdown" %>

<%
    //WestWindSiteUtils.LogInfo("/WebMonitor/default.aspx");	
%>
<!DOCTYPE html>
<html>
<head>
    <title>Purchase Markdown Monster</title>
    <meta name="description" content="Purchase Markdown Monster. Markdown Monster: An easy to use Markdown Editor and Weblog Publishing tool for Windows. Create Markdown with a low key interface that gets out of your way, but provides advanced features to help you be more productive." />
    <meta name="keywords" content="markdown, windows, markdown editor,text editor,documentation,weblog,publishing,screen capture,writing,documentation,open source,extensible,addins,wpf,dotnet,westwind,rick strahl" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    
    <meta name="company" content="West Wind Technologies - https://west-wind.com"/>
    <meta name="author" content="Rick Strahl, West Wind Technologies  - https://weblog.west-wind.com"/>

    <link href="https://netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.css" rel="stylesheet" />
    <link href="https://netdna.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.css" rel="stylesheet" />
    
    <style>     
        th {
            background: #535353;
            color: whitesmoke;
            text-align: center;
        }
        td a {
            font-weight: bold;
        }
        .price {
            text-align: right;
            font-weight: bold;
            font-size: 1.3em;
            color: maroon;
        }
        #PricingTable td .btn-primary {
            width: 95px;
        }
        .centered {
            text-align: center;                
            background: #b4fae1;    
        }
        .centered .fa {
            color: #02b402; 

        }
        .centered.not-available 
        {
            background: white;
        }
        #FeatureTable td {
            min-width: 75px;
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
    <span class="banner-title" style="cursor: pointer;" onclick="window.location = './';">
        <img src="Images/MarkdownMonster_Icon_32.png"
             style="height: 28px;"
             alt="Markdown Monster" />
        <span class="hidable-xs">Markdown Monster</span>
    </span>
    <nav id="TopMenu" class="right">
        <a href="./">
            Home
        </a>
        <a href="https://www.youtube.com/watch?v=XjFf57Ap9VE" >                
            Video
        </a>        
        <a href="download.aspx" class="hidable" title="Download Markdown Monster">                
            Download
        </a>
        <a href="Purchase.aspx" title="Purchase Markdown Monster" class="active">                
            Buy
        </a>                 
        <a href="https://support.west-wind.com/Thread4NM0M17RC.wwt" class="hidable" >                
            Support
        </a>
        <a href="docs/">                
            Docs
        </a>
        <a href="https://medium.com/markdown-monster-blog/" title="Markdown Monster Weblog">                
            Blog
        </a>
        <%--<a href="pricing.aspx">License</a>--%>
    </nav>
</div>
    

    <div id="MainContainer" class="background">
        <div id="ContentContainer" class="content" style="padding: 10px 45px;">

<ww:Markdown runat="server" id="license" NormalizeWhiteSpace="true">
## Purchase Markdown Monster

Markdown Monster can be [downloaded](download.aspx) and evaluated for free and we 
provide the [source code](https://github.com/RickStrahl/MarkdownMonster) 
in the open, but a reasonably priced license must be purchased for continued use. 
    
We offer individual and organizational licences to purchase, as well as **free** licenses to contributors 
and certain developer organizations. For more licensing information, please check out 
our [Licensing Page](download.aspx#License).
    
Thank you for playing fair.
</ww:Markdown>
            
            <hr />
            
  
            <h3 style="margin:10px 0 20px">Markdown Monster Licenses</h3>
            <table id="PricingTable" class="table table-bordered table-striped" style="width: 90%;">
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tr>
                    <td>
                        <a href="download.aspx" class="pull-right btn btn-sm btn-primary">
                            <i class="fa fa-play-circle"></i>
                            Try out now
                        </a>
                        <a href="download.aspx">Markdown Monster Trial</a><br/>
                        <small>(for checking out Markdown Monster features)</small>
                    </td>
                    <td class="price">
                        FREE
                    </td>
                </tr>
                <tr>
                    <td>
                        <a href="https://store.west-wind.com/product/order/markdown_monster" 
                            class="pull-right btn btn-sm btn-primary"
                            title="Buy Markdown Monster Single License">
                            <i class="fa fa-credit-card"></i>
                            Buy
                        </a>
                        <a href="https://store.west-wind.com/product/order/markdown_monster">Markdown Monster Single User</a><br/>
                        <small>Single User License</small>
                    </td>
                    <td class="price">                                                
                        $39                        
                    </td>
                </tr>
           
                <tr>
                    <td>
                        <a href="https://store.west-wind.com/product/order/markdown_monster_5user" 
                           class="pull-right btn btn-sm btn-primary" 
                           title="Buy Markdown Monster Single License">
                            <i class="fa fa-credit-card"></i>
                            Buy
                        </a>
                        <a href="https://store.west-wind.com/product/order/markdown_monster_5user">Markdown Monster 5 User License</a><br/>
                        <small>5 Users</small>
                    </td>
                    <td class="price">                                                
                        $169                        
                    </td>
                </tr>
                <tr>
                    <td>
                        <a href="https://store.west-wind.com/product/order/markdown_monster_10user" 
                           class="pull-right btn btn-sm btn-primary" 
                           title="Buy Markdown Monster Single License">
                            <i class="fa fa-credit-card"></i>
                            Buy
                        </a>
                        <a href="https://store.west-wind.com/product/order/markdown_monster_10user">Markdown Monster 10 User License</a><br/>
                        <small>10 Users</small>
                    </td>
                    <td class="price">                                                
                        $299                       
                    </td>
                </tr>
                <tr>
                    <td>
                        <a href="https://store.west-wind.com/product/markdown_monster_site" 
                            class="pull-right btn btn-sm btn-primary"
                            title="Buy Markdown Monster Organizational License">
                            <i class="fa fa-credit-card"></i>
                            Buy
                        </a>

                        <a href="https://store.west-wind.com/product/markdown_monster_site">Markdown Monster Site License</a><br/> 
                        <small>Site license that can be used by any number of users within a single organization.</small>                       
                    </td>
                    <td class="price">                                                
                        $899                        
                    </td>
                </tr>
                

                </table>
            
          
            <p>
                Product purchases can be made through our secure online store. For more information on other order arrangements please visit our order policies page.

                <ul>
                    <li><a href="http://store.west-wind.com/products?search=markdownmonster">Purchase in our online Store</a></li>
                    <li><a href="Download.aspx#License">License Information</a></li>
                    <li><a href="http://west-wind.com/pricing.aspx">Order Policies</a></li>
                </ul>
    
            </p>
            
            <h3>Warranty Disclaimer: No Warranty!</h3>
            <p style="margin-bottom: 50px;">
                IN NO EVENT SHALL THE AUTHOR, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR
                REDISTRIBUTE THIS PROGRAM AND DOCUMENTATION, BE LIABLE FOR ANY COMMERCIAL, SPECIAL, 
                INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY TO USE
                THE PROGRAM INCLUDING, BUT NOT LIMITED TO, LOSS OF DATA OR DATA BEING RENDERED INACCURATE 
                OR LOSSES SUSTAINED BY YOU OR LOSSES SUSTAINED BY THIRD PARTIES OR A FAILURE OF THE PROGRAM 
                TO OPERATE WITH ANY OTHER PROGRAMS, EVEN IF YOU OR OTHER PARTIES HAVE BEEN ADVISED 
                OF THE POSSIBILITY OF SUCH DAMAGES.
            </p>
            
            

        </div>
        <div class="clearfix"></div>
        
        <nav class="banner banner-bottom" style="margin-top: 90px;">
            <div class="right">
                created by:<br />
                <a href="http://west-wind.com/" style="padding: 0;">
                    <img src="Images/wwToolbarLogo.png" style="width: 150px;" />
                </a>
            </div>
            &copy; West Wind Technologies, <%= DateTime.Now.Year %>
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
