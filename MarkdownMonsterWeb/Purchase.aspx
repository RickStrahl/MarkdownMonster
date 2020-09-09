<%@ Page Language="C#" %>
<%@ Register TagPrefix="ww" Namespace="Westwind.Web.Markdown" Assembly="Westwind.Web.Markdown" %>

<%
    //WestWindSiteUtils.LogInfo("/WebMonitor/default.aspx");	
%>
<!DOCTYPE html>
<html>
<head>
    <title>Purchase Markdown Monster</title>
    <meta charset="utf-8" /> 
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />   


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
        <div id="ContentContainer" class="content" >
            
        <div class="row">
        <div class=" col-sm-2" style="padding: 37px 10px 1px">
            <img src="Images/MarkdownMonster_Icon_256.png" style="display: block; width: 210px"/>
        </div>
        <div class=" col-sm-10">
<ww:Markdown runat="server" id="license" NormalizeWhiteSpace="true">
## Purchase Markdown Monster

Markdown Monster can be [downloaded](download.aspx) and evaluated for free. We also 
provide the [source code](https://github.com/RickStrahl/MarkdownMonster) 
in the open, but a reasonably priced license must be purchased for continued use. 
    
We offer individual and organizational licenses to purchase, as well as **free** licenses to contributors 
and certain developer organizations. Discounted licenses are available for some International locations for fair global pricing. 
For more licensing information, please check out [Licensing Information](#License) below.
    
Thanks for playing fair.
</ww:Markdown>
             </div>
            </div>

            <hr />
            
  
            <h3 id="Licenses" style="margin:10px 0 20px">Markdown Monster Licenses</h3>
            <table id="PricingTable" class="table table-bordered table-striped" style="width: 100%;margin-bottom: 0;">
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tr>
                    <td>
                        <a href="download.aspx" class="pull-right btn btn-sm btn-success">
                            <i class="fa fa-download"></i>
                            Download
                        </a>
                        <a href="download.aspx">Markdown Monster Evaluation</a><br/>
                        <small>try it out, check out features, fully functional.</small>
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
                        <small>Single User License (valid for multiple machines)</small>
                    </td>
                    <td class="price">                                                
                        $49                        
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
                        <small>5 User License</small>
                    </td>
                    <td class="price">                                                
                        $199                        
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
                        <small>10 User License</small>
                    </td>
                    <td class="price">                                                
                        $379                       
                    </td>
                </tr>
                <tr>
                    <td>
                        <a href="https://store.west-wind.com/product/order/markdown_monster_25user" 
                           class="pull-right btn btn-sm btn-primary" 
                           title="Buy Markdown Monster 25 User License">
                            <i class="fa fa-credit-card"></i>
                            Buy
                        </a>
                        <a href="https://store.west-wind.com/product/order/markdown_monster_25user">Markdown Monster 25 User License</a><br/>
                        <small>25 User License</small>
                    </td>
                    <td class="price">                                                
                        $999                       
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
                        $2,299                        
                    </td>
                </tr>

            </table>
        <p style="margin-top: 0">
            <small><i><super>*</super> <a href="#discountpricing">discounted pricing available</a> for various global locations</i></small>
        </p> 
       
                <p>
                    For more information on order arrangements please visit <a href="https://west-wind.com/pricing.aspx#purchasing-west-wind-products">our order policies page</a>.
                </p>

                
            
            
            <div id="License" class="panel panel-default"
                 style="margin-top: 1.5em;margin-bottom: 50px;">

            <div class="panel-heading" >                    

            <h5 class="panel-title">Licensing</h5>
            </div>
            <div class="panel-body">

     <ww:Markdown runat="server" id="Markdown1" NormalizeWhiteSpace="true">

Although we provide the <a href="https://github.com/RickStrahl/MarkdownMonster">source in the open</a> on GitHub,
Markdown Monster is <b>licensed software</b>. Markdown Monster can be downloaded and evaluated for free,
but [a reasonably priced license](https://store.west-wind.com/product/order/markdown_monster) must be purchased for continued use. 

<a href="#License">More detailed license information can be found below</a>.
                    
**Thank you for playing fair.**
         
 <p>
     <a href="#Licenses" class="btn btn-primary" style="width: 250px">
         <i class="fa fa-check" style="color: lightgreen"></i>&nbsp;
         Purchase a License
     </a>
 </p>


        
#### Contribute - Get a Free License{#Contribute}
Contributors that provide valuable feedback with quality bug reports or enhancement requests, 
help out with code via Pull Requests, help promote Markdown Monster, or support Markdown Monster in
any other significant way are all eligible for a free license.
    
<a href="http://west-wind.com/contact/" target="top">Contact Rick for more info</a>
or - just as likely - I'll be contacting you.
         
#### MVPS, ASP.NET Insiders and Microsoft Employees Get a Free License				   
Microsoft MVPs, ASP.NET Insiders and Microsoft employees as well as employees of any company offering free tools to 
the Microsoft MVP program also qualify for a free license.
	            
<a class="btn btn-primary" 
   href="https://store.west-wind.com/mvpperks.aspx" target="top" style="width: 250px">
    <i class="fa fa-id-card-o" style="color: #ddd"></i>&nbsp;
    Apply for a free MVP license
</a>.


#### Globally Adjusted Discount Pricing {#discountpricing}
Live in a country where pricing for Markdown Monster is prohibitive? Let us help you get licensed with a discounted
license with adjusted international pricing for your location. 
         
<a class="btn btn-primary" href="https://store.west-wind.com/discountpricing.aspx" target="_blank" style="width: 250px">
    <i class="fa fa-chevron-circle-down" style="color: #ddd"></i>&nbsp;
    Apply for a Discounted License
</a>


--- 
         
<h3 >Markdown Monster License</h3>
         
Markdown Monster comes in several license modes: Evaluation, Single User, Multiple User and Site License.

Markdown Monster is **Source Open** with source code available on GitHub, but it is a licensed product that requires a paid-for license for continued use. The software is licensed as © Rick Strahl, West Wind Technologies, 2015-2020. 

A fully functional, free evaluation version is available for evaluation use, but continued use requires purchase of a license. 

Licenses can be purchased from:  
https://store.west-wind.com/product/markdown_monster


#### Evaluation License

The Evaluation version has all the features and functionality of the registered version, except that it shows occasional freeware notices. Tampering with or removing of the notices is not allowed with the evaluation license.

You can use the evaluation version with the notices enabled, but if you use Markdown Monster regularly or for commercial use, please register and support further development and maintenance.

#### Purchased License

For continued use or commercial use of Markdown Monster a paid-for license is required. The paid-for license removes the freeware notices.

Each licensed user must have a separate license, but a single user may use multiple copies of Markdown Monster on multiple machines.

The multi-user licenses work the same as a single user license applied to the number of users specified on the license purchased. An organizational site license is available to allow any number of users running unlimited numbers of Markdown Monster instances within a single commercial, non-profit or government organization.

Any purchased license is valid for the duration of the major release that it was purchased for (ie. 1.00-1.99) and minor version updates within that major version are always free. Upgrade pricing is available for major version upgrades, usually at half of full price, and it's our policy to allow for free upgrades to the next major version within a year of purchase.

#### Source Code

Markdown Monster is **Source Open** and source code is available on [GitHub](https://github.com/RickStrahl/MarkdownMonster), but the licensing outlined above applies to the source code use as well as the binary distributions. We allow modification of source code for internal use of Markdown Monster in your organization or for submitting pull requests to the Markdown Monster main repository. Under no circumstances are you allowed to re-package and re-distribute any part of Markdown Monster outside of your organization.

#### Help us out - Get a free License

We encourage pull requests for feature suggestions or bug fixes to be submitted back to the Markdown Monster repository. Please check with us first before comitting to providing larger PRs. Any contributors that provide meaningful enhancements, help with identifying and or fixing of bugs or by actively promoting Markdown Monster can qualify for a free license (at our discretion). Additionally Microsoft MVPs and Insiders and Microsoft Employees can [apply for a free license](https://markdownmonster.west-wind.com/purchase.aspx#contribute---get-a-free-license").

### Warranty Disclaimer: NO WARRANTY!

YOU EXPRESSLY ACKNOWLEDGE AND AGREE THAT USE OF THE LICENSED APPLICATION IS AT YOUR SOLE RISK AND THAT THE ENTIRE RISK AS TO SATISFACTORY QUALITY, PERFORMANCE, ACCURACY AND EFFORT IS WITH YOU. TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THE LICENSE APPLICATION AND ANY SERVICES PERFORMED OR PROVIDED BY THE LICENSED APPLICATION ("SERVICES") ARE PROVIDED "AS IS" AND "AS AVAILABLE," WITH ALL FAULTS AND WITHOUT WARRANTY OF ANY KIND, AND APPLICATION PROVIDER HEREBY DISCLAIMS ALL WARRANTIES AND CONDITIONS WITH RESPECT TO THE LICENSED APPLICATION AND ANY SERVICES, EITHER EXPRESS, IMPLIED OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES AND/OR CONDITIONS OF MERCHANTABILITY, OF SATISFACTORY QUALITY, OF FITNESS FOR A PARTICULAR PURPOSE, OF ACCURACY, OF QUIET ENJOYMENT, AND NON-INFRINGEMENT OF THIRD PARTY RIGHTS. APPLICATION PROVIDER DOES NOT WARRANT AGAINST INTERFERENCE WITH YOUR ENJOYMENT OF THE LICENSED APPLICATION, THAT THE FUNCTIONS CONTAINED IN, OR SERVICES PERFORMED OR PROVIDED BY, THE LICENSED APPLICATION WILL MEET YOUR REQUIREMENTS, THAT THE OPERATION OF THE LICENSED APPLICATION OR SERVICES WILL BE UNINTERRUPTED OR ERROR-FREE, OR THAT EFFECTS IN THE LICENSED APPLICATION OR SERVICES WILL BE CORRECTED. NO ORAL OR WRITTEN INFORMATION OR ADVICE GIVEN BY APPLICATION PROVIDER OR ITS AUTHORIZED REPRESENTATIVE SHALL CREATE A WARRANTY. SHOULD THE LICENSED APPLICATION OR SERVICES PROVE DEFECTIVE, YOU ASSUME THE ENTIRE COST OF ALL NECESSARY SERVICING, REPAIR OR CORRECTION.

IN NO EVENT SHALL THE AUTHOR, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR REDISTRIBUTE THIS PROGRAM AND DOCUMENTATION, BE LIABLE FOR ANY COMMERCIAL, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY TO USE THE PROGRAM INCLUDING, BUT NOT LIMITED TO, LOSS OF DATA OR DATA BEING RENDERED INACCURATE OR LOSSES SUSTAINED BY YOU OR LOSSES SUSTAINED BY THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE WITH ANY OTHER PROGRAMS, EVEN IF YOU OR OTHER PARTIES HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
                         
     </ww:Markdown>

            </div>

            </div>

        </div>

        <div class="clearfix"></div>
        
        <nav class="banner banner-bottom" style="margin-top: 50px;">
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
