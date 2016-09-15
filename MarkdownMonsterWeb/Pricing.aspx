<%@ Page Language="C#" %>

<%
    //WestWindSiteUtils.LogInfo("/WebMonitor/default.aspx");	
%>
<!DOCTYPE html>
<html>
<head>
    <title>Pricing for Markdown Monster</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <meta name="description" content="West Wind Markdown Monster: An easy to use and extensible Markdown editor for Windows" />
    <meta name="keywords" content="Markdown,Editor,Editing,Weblog,Writing,Documentation" />

    <link href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.css" rel="stylesheet" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet" />
    
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
</head>
<body>
    <div class="banner">                  
         <div class="banner-title" style="cursor: pointer;" onclick="window.location = './';" >
            <img src="Images/websurgeicon.png" 
                style="height: 28px;"
                alt="West Wind Globalization" /> 
            <span>West Wind WebSurge</span>
        </div>        
        <div id="TopMenu" class="right">
            <a href="./" >Home</a>            
            <a href="https://youtu.be/O5J8mDfVZH8">Video</a>
            <a href="features.aspx">Features</a>
            <a href="pricing.aspx" class="active">License</a>
            <a href="download.aspx">Download</a>                        
        </div>       
    </div>
    

    <div id="MainContainer" class="background">
        <div id="ContentContainer" class="content" style="padding: 10px 45px;">

            <h2 style="color: steelblue">Licensing and Pricing of West Wind WebSurge</h2>
            <p>
                West Wind WebSurge is open source with source code <a href="https://github.com/rickstrahl/WestwindWebSurge">available on GitHub</a>, 
                but it is a licensed product.                                                 
                A <b>free</b> version is available that can be used without explicit registration 
                or usage limits for <b>personal use</b> and checking out of WebSurge's features.
            </p>
                
            <p>
                For commercial use or continued personal use, please purchase a Professional or 
                Organizational license. Each licensed user requires a separate license, but a 
                single user may use multiple copies of West Wind WebSurge on multiple machines, 
                given that only one copy at a time is in use. An organizational license is 
                available to allow any number of users running unlimited numbers of 
                West Wind WebSurge within a single organization. Any purchased license is 
                valid for the duration of the major release that it was purchased for (ie. 1.00-1.99). 
            </p>
            
            <p>
                We provide the fully functional, unlimited version of West Wind WebSurge for download,
                so we rely on the honor system from users for supporting this product. </p>
            <p>
                <b>Thank you for playing fair!</b>
            </p>
<%--            <p>
                For more licensing information please check out the 
                <a href="http://websurge.west-wind.com/docs/?page=_433179ec8.htm">
                    West Wind WebSurge Licensing
                </a> topic in the documentation.                
            </p>--%>
                                 
            <table class="table table-bordered table-striped ta" style="width: 80%">
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tr>
                    <td>
                        <a href="download.aspx">WebSurge Free</a><br/>
                        <small>(for personal use or checking out WebSurge features)</small>
                    </td>
                    <td class="price">
                        <span style="font-weight: bold; color: maroon">FREE</span>
                    </td>
                </tr>
                <tr>
                    <td>
                        <a href="http://store.west-wind.com/product/websurge" class="pull-right btn btn-sm btn-primary" style="width: 70px;">
                            <i class="fa fa-credit-card"></i>
                            Buy
                        </a>
                        <a href="http://store.west-wind.com/product/websurge">WebSurge Professional</a><br/>
                        <small>(required for commercial use, suggested for continued personal use)</small>
                        
                    </td>
                    <td class="price">
                        $149.00
                    </td>
                </tr>
                <tr>
                    <td>
                        <a href="http://store.west-wind.com/product/websurge" class="pull-right btn btn-sm btn-primary" style="width: 70px;">
                            <i class="fa fa-credit-card"></i>
                            Buy
                        </a>

                        <a href="http://store.west-wind.com/product/websurge_org">WebSurge Organizational License</a><br/>
                        <small>(unlimited users for a single organization)</small>
                    </td>
                    <td class="price">
                        $799.00
                    </td>
                </tr>
                </table>
            
            <p>
                Product purchases can be made through our secure online store. For more information on other order arrangements please visit our order policies page.

                <ul>
                    <li><a href="http://store.west-wind.com/products?search=Web+Surge">Purchase in our online Store</a></li>
                    <li><a href="http://west-wind.com/pricing.aspx">Order Policies</a></li>
                </ul>
    
            </p>
            <h3>Feature Matrix</h3>
            <a name="FeatureMatrix"></a>
            <p>
                Not sure which version to get? Here's a feature matrix that can help you decide.
            </p>
            <table id="FeatureTable" class="table table-bordered" style="width: 80%">
                <thead>
                    <tr>
                        <th>Feature</th>
                        <th>Free</th>
                        <th>Professional</th>
                        <th>Org</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>
                            Max number of 
                            URLs in session
                        </td>
                        <td class="centered">
                            unlimited   
                        </td>
                        <td class="centered">
                            unlimited
                        </td>
                        <td class="centered">unlimited
                        </td>
                    </tr>
                     <tr>
                        <td>
                            Max number of 
                            simultaneous sessions
                        </td>
                        <td class="centered">
                            unlimited   
                        </td>
                        <td class="centered">
                            unlimited
                        </td>
                        <td class="centered">unlimited
                        </td>
                    </tr>
                       
                    <tr>
                        <td>
                            Built-in HTTP Capture Tool</td>
                        <td class="centered">
                                <i class="fa fa-check fa-2x"></i>
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Capture any Windows HTTP traffic from Web Browsers or Apps</td>
                         <td class="centered">
                                <i class="fa fa-check fa-2x"></i>
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                                        <tr>
                        <td>
                            Filter captures by domain, process or user defined exclusions</td>
                         <td class="centered">
                                <i class="fa fa-check fa-2x"></i>
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Support for advanced HTTP features and SSL</td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>    
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Support for <a href="http://telerik.com/fiddler">Telerik&#39;s Fiddler Exports</a>
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>    
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Export results to JSON, XML, HTML
                            and raw HTTP Headers
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>    
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Test locally within your Firewall or VPN
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>    
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                    <tr>
                        <td>
                            Multiple Users within an Organization
                        </td>
                        <td class="centered not-available">
                                &nbsp;
                        </td>
                        <td class="centered not-available">
                            &nbsp;
                        </td>
                        <td class="centered">
                            <i class="fa fa-check fa-2x"></i>  
                        </td>
                    </tr>
                </tbody>
            </table>
            


        </div>
        <div class="clearfix"></div>
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
      (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
      (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
      m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
      })(window,document,'script','https://www.google-analytics.com/analytics.js','ga');

      ga('create', 'UA-9492219-14', 'auto');
      ga('send', 'pageview');
    </script>
</body>

</html>
