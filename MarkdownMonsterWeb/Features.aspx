<%@ Page Language="C#" %>
<%@ Register TagPrefix="ww" Namespace="Westwind.Web.MarkdownControl" Assembly="Westwind.Web.MarkdownControl" %>

<%
    //WestWindSiteUtils.LogInfo("/WebMonitor/default.aspx");	
%>
<!DOCTYPE html>
<html>
<head>
    <title>Features - West Wind WebSurge</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <meta name="description" content="West Wind Web Surge Features. A brief walk through of functionality with many screenshots to get a feel for operation of West Wind Web Surge" />
    <meta name="keywords" content="Load,Testing,Web,Server,ASP.NET,Capture,Playback,Threads" />

    <link href="//netdna.bootstrapcdn.com/bootstrap/3.1.1/css/bootstrap.min.css" rel="stylesheet" />
    <link href="Css/application.css" rel="stylesheet" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet" />
</head>
<body>
    <a id="top" name="top" style="height: 0;"></a>
    <div class="banner">
        <div class="banner-title" style="cursor: pointer;" onclick="window.location = './';">
            <img src="Images/websurgeicon.png"
                style="height: 28px;"
                alt="West Wind Globalization" />
            <span>West Wind WebSurge</span>
        </div>
        <div id="TopMenu" class="right">
            <a href="./">Home</a>
            <a href="https://youtu.be/O5J8mDfVZH8">Video</a>
            <a href="features.aspx" class="active">Features</a>
            <a href="download.aspx">Download</a>
            <a href="pricing.aspx">License</a>            
        </div>
    </div>


    <article id="MainContainer" class="background">
        <div id="ContentContainer" class="content" style="padding: 10px 45px;">

            <h2 style="color: steelblue">West Wind Web Surge Features</h2>
            <p>
                The following is an overview of features along with a number of screen shots to
                demonstrate WebSurge&#39;s features.
            </p>
            <ul>
                <li><a href="#capturing">Request Capturing</a></li>
                <li><a href="#loadtesting">Load Testing</a></li>
                <li><a href="#commandline">Command Line Operation</a></li>
            </ul>
            
            <ww:Markdown runat="server" id="md1">
<a name="capturing"></a>                
### Request Capturing
                
Effective URL and load testing testing starts with capturing or creating
URL sessions that can be managed and saved easily. WebSurge makes this process as
easy as possible to remove as much friction from the process as possible to 
encourage frequent creation of sessions to run tests on.
                
WebSurge lets you capture or enter full HTTP request data in a number
of different ways:
                
* Built in HTTP Proxy Capture
* Manual entry of HTTP Request through the UI
* HTTP Request Traces in a Text File
                
### Using the HTTP Proxy Capture Tool
                
The easiest ways to capture URLs that make up an HTTP session is by using
the built in URL capture tool. It uses an HTTP proxy to monitor your HTTP
requests on your machine and automatically captures the request data into
HTTP session files.
Using the capture tool (File | Capture Session or click the Capture Button on 
the Session View) brings up the capture form which looks like this:                
            </ww:Markdown>
            
            <h3>Request Capturing</h3>
            <p>
                Effective URL and load testing testing starts with capturing or creating
            URL 
                sessions that can be managed and saved easily. WebSurge makes this process as
            easy as possible to remove as much friction from the process as possible to 
                encourage frequent creation of sessions to run tests on.
                <p>
                    WebSurge lets you capture or enter full HTTP request data in a number
                of different ways:
                <ul>
                    <li>Built in HTTP Proxy Capture</li>
                    <li>Manual entry of HTTP Request through the UI</li>
                    <li>HTTP Request Traces in a Text File</li>
                </ul>

                    <h4>Using the HTTP Proxy Capture Tool</h4>
                    <p>
                        The easiest ways to capture URLs that make up an HTTP session is by using
                    the built in URL capture tool. It uses an HTTP proxy to monitor your HTTP
                    requests on your machine and automatically captures the request data into
                    HTTP session files.
                    Using the capture tool (File | Capture Session or click the Capture Button on 
                    the Session View) brings up the capture form which looks like this:<br />
                    </p>

                    <img class="media-object" src="Images/WebSurge_Capture.png" />

                    <p>
                        To start capturing just press capture and HTTP traffic from your machine will 
                    immediately be captured. There are a number of filters that allow you to filter
                    by domain (great for creating local tests that avoid capturing HTTP chatter from machine
                        services or social media apps as well as avoiding capture of external resources),
                        process Id
                    (tie it to a browser instance or application), as well as a filter option to not capture
                        static image, css and javascript resources.                    
                    </p>
                    <p>
                        As you capture the capture window shows a live tally of captured requests. When
                        done press
                    <i>Stop Capture</i> to stop the capture. Once done you can then save the captured data
                        to a
                    file which becomes a new session file that is also automatically opened in the main
                        WebSurge
                    testing window.
                    </p>


                    <h4>Manually creating Tests</h4>
                    <p>
                        Of course you can also create request data manually using the WebSurge Request entry
                        form. This
                    form makes it easy to create requests by hand. You can add all HTTP specific information
                        including
                    URL, headers and content for each requests.
                    </p>

                    <img src="Images/WebSurge_RequestEntry.png" />

                    <p>
                        Note that you can easily add any kind of HTTP header or content. This allows you
                        to create either plain GET requests, or more complex POST/PUT/DELETE requests that
                        actually push data to the server. Since all of HTTP features are available here
                        you 
                    can easily create any type of HTTP request.
                    </p>
                    <p>
                        Once you've entered a new request click the Save Button which automatically adds
                        the
                    entry to the current session displayed in the list on the left. Click the Save button
                        on the Session window to save the Session to disk at any time.
                    </p>
                    <p>
                        You can also easily copy an existing request to a new request. Frequently when creating
                        request urls by hand headers and content are very similar between requests and this
                        option
                    makes it easy to quickly modify an existing request to create a new one.
                    </p>
                    <p>
                        You can also easily test requests by clicking Test button or Alt-T on the Session
                        window 
                    to run the selected request and display the results. This is great for quick individual
                        URL testing, or even for testing REST APIs during development or adoption of an
                        existing 
                    API from a vendor.
                    </p>
                    <img src="Images/WebSurge_RequestDisplay.png" />

                    <p>
                        The Test form displays response headers and the raw response of a request. If an
                        error occurrs
                    the HTTP error message and status code is captured and displayed prominently on top
                        of the entry.
                    Notice the <i>Show html</i> button on the result form - it'll let you view rendered
                        HTML which
                    sometimes facilitates understanding the result rather than looking at a HTML text display.
                        Likewise
                    JSON and XML responses can be displayed as pretty formatted documents that are easier
                        to read than
                    the raw data.
                    </p>

                    <h4>Header Overrides for Authentication and Cookies</h4>
                    <p>
                        One important thing that happens when you create or capture requests is that headers
                        get 
                        created with token from the current capture or manual entry. However, cookies and
                        auth tokens
                        expire, so existing captured sessions go stale and test that worked previously no
                        longer work.                        
                    </p>
                    <p>
                        In order to get around this you can override various headers and request values
                        via the options
                        that are tied to a session as a whole. For example, you can force replacement of
                        Cookie values or
                        Authorization header, replace query string values or override the domain for each
                        request.                       
                    </p>

                    <img src="Images/WebSurge_ReplacementOptions.png" />

                    <p>
                        The values set in this dialog are Session specific and are saved along with the
                        Session HTTP
                        data when you save a Session. 
                    </p>
                    <p>
                        To use this feature when your cookies/auth headers have expired, you can
                        capture a valid request that has the appropriate cookie and/or authentication header.
                        You can
                        use the WebSurge Capture tool, any Web Browser DevTools (f12 tools) or an HTTP proxy
                        like 
                        Fiddler to get the updated header values. Simply copy the header values and paste
                        them into 
                        the options form. Then re-run your tests - the options values override the originally
                        captured
                        values for each request tested.
                    </p>

                    <h4>HTTP Trace File</h4>
                    <p>
                        You can also enter requests into WebSurge on a lower level by directly manipulating
                        the raw
                        session trace file. Web Surge stores Session data as raw text files that are simply
                        delimited
                        HTTP headers. The format used is also shared by Telerik's Fiddler, and simply contains
                        raw
                        HTTP headers with a delimiter between requests.
                    </p>

                    <img src="Images/WebSurge_SessionFile.png" />

                    <p>
                        You can manually create sessions simply by creating or editing these text files
                        either by hand
                        or under program control. From within WebSurge you can click the Edit button on
                        the Session
                        tab to bring up the raw session file. If WebSurge is running and you opened the
                        file from
                        within it any changes you make and save to disk are also immediately reflected and
                        applied 
                        back in the Session list in the UI.
                    </p>
                    <p>
                        We believe that one of the biggest obstacles to Load Testing and URL testing in
                        general
                        is usually the process of creating sessions. By providing several different approaches
                        we give you lots of options from automating capture from applications, to manually
                        creating
                        requests either in the UI or in the text files directly, or by allowing you to generate
                        requests easily using any tool that can write out a text file.                        
                    </p>
                    <p>
                        There's no excuse, so "Get testing!"
                    </p>

                    <a name="loadtesting"></a>
                    <h3>Load Testing</h3>
                    Load Testing is Web Surge's main feature of course is load testing. 
                Running tests is very straight forward: Fill in the number of seconds
                to run the test for and the number of simultaneous session you want to
                run. Then click <i>start</i> and off you go.
                </p>

            <img src="Images/WebSurge_RequestsView.png" />

            <p>
                As you run the test you see a console view on the right that shows active
                requests being process along with a running total in the lower right 
                hand corner. The console view can be turned off for high volume tests
                that generate 1000's of requests a second to minimize CPU load of the
                application but for request/sec of under a couple of thousand the 
                display is fun to watch.            
            </p>
            <p>
                On the bottom is a summary display that shows the number of requests processed
                along with an average request per second calculation for the test run. The running
                total shows in green if there are no errors, but if error requests are hit changes
                to red.
            </p>
            <p>
                Once the test is done you'll get to see a summary results view:
            </p>

            <img src="Images/WebSurge_ResultsView.png" />

            <p>
                The results view summarizes the entire run at the top, and then also displays information
                for each individual requests show average request length and the number of requests
                that
                succeeded or failed.
            </p>
            <p>
                To drill into individual requests click the <i>Results</i> tab on the left and click
                on
                any entry in the list which brings up a detailed raw HTTP view of that request and
                response.
            </p>
            <img src="Images/WebSurge_ResultView.png" />
            <p>
                You can review both the request and response headers and you can click on the original
                URL
                to directly access the request (realistic only for GET requests). You can also click
                on the
                <i>Show html/json/xml</i> link to display either the rendered HTML or pretty formatted
                JSON or XML.
                Note that only a subset of requests is displayed and editable in this fashion.                
            </p>
            <p>
                You can also click the Graph link to quickly view the a requests per second view
                over time of
                the test.
            </p>
            <img src="Images/WebSurge_Charts.png" />

            <p>
                Other charts let you see request times charted either for the whole session or for
                individual
                requests. 
            </p>
            <p>
                Finally you can also export the result of a test to a raw HTTP trace, JSON or XML
                document.
                HTTP trace results can be re-imported so that you can re-examine previous test runs
                by using
                the <i>Import WebSurge Results</i> from the results tab's context menu.
            </p>

            <a name="commandline"></a>
            <h3>Command Line Operation</h3>

            <p>
                WebSurge also supports command line operation, so you can automate load tests from
                other
                operations or development environments.
            </p>

            <p>
                Command line operation allows running tests for individual URLs very quickly:
            </p>

            <img src="Images/Console.png" /><br />

            or you can run a full session file:
                      
            <pre>c:\> websurgecli c:\temp\MyDailyDosha_Session.txt -t20 -s10</pre>

            <p>
                There are a number of options available to control how long the test runs (-s)
                and how many instances are used (-t). You can also control how the output is 
                handled, either continous, minimal or just the summary.
            </p>

            <img src="Images/WebSurgeConsole_Options.png" />

            <p>
                Commandline operation allows you to very quickly test a single URL or an
                existing session with just a single command. You can also easily integrate
                WebSurge into a build process by running short tests and checking for failures
                and or slow response times beyond expected values.
            </p>

            <div class="panel-footer">
                <a href="#top">Back to top</a>
            </div>
        </div>
    </article>

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

    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js" type="text/javascript"></script>
    <%--<script src="/Scripts/scroll-to-hash.js"></script>--%>
    <script>        
        (function () {
            // Smooth scroll for in page links

            var target, scroll;

            $("a[href*=#]:not([href=#])").on("click", function (e) {

                console.log('scrolling');
                debugger;

                if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && location.hostname == this.hostname) {
                    target = $(this.hash);
                    target = target.length ? target : $("[id=" + this.hash.slice(1) + "],[name=" + this.hash.slice(1) + "]");


                    if (target.length) {
                        if (typeof document.body.style.transitionProperty === 'string') {
                            e.preventDefault();

                            var avail = $(document).height() - $(window).height();

                            scroll = target.offset().top;

                            if (scroll > avail) {
                                scroll = avail;
                            }

                            $("html").css({
                                "margin-top": ($(window).scrollTop() - scroll) + "px",
                                "transition": "1s ease-in-out"
                            }).data("transitioning", true);
                        } else {
                            $("html, body").animate({
                                scrollTop: scroll
                            }, 1000);
                            return;
                        }
                    }
                }
            });

            $("html").on("transitionend webkitTransitionEnd msTransitionEnd oTransitionEnd", function (e) {

                if (e.target == e.currentTarget && $(this).data("transitioning") === true) {
                    $(this).removeAttr("style").data("transitioning", false);
                    $("html, body").scrollTop(scroll);
                    return;
                }
            });
        })();
    </script>
    <script>
        (function (i, s, o, g, r, a, m) {
            i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
                (i[r].q = i[r].q || []).push(arguments)
            }, i[r].l = 1 * new Date(); a = s.createElement(o),
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
        })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

        ga('create', 'UA-9492219-10', 'auto');
        ga('send', 'pageview');

    </script>
</body>

</html>
