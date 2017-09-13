/// <reference path="../bower_components/jquery/dist/jquery.js" />
/*
ww.jQuery.js  
Version 1.26 - 2/2/2016
West Wind jQuery plug-ins and utilities

(c) 2008-2015 Rick Strahl, West Wind Technologies 
www.west-wind.com

Licensed under MIT License
http://en.wikipedia.org/wiki/MIT_License
*/
(function($, undefined) {
    HttpClient = function(opt) {
        var self = this;

        this.completed = null;
        this.errorHandler = null;
        this.errorMessage = "";
        this.async = true;
        this.evalResult = false; // treat result as JSON 
        this.contentType = "application/x-www-form-urlencoded";
        this.accepts = null;
        this.method = "GET";
        this.timeout = 20000;
        this.headers = {};

        $.extend(self, opt);

        this.appendHeader = function(header, value) {
            self.headers[header] = value;
        };
        this.send = function(url, postData, completed, errorHandler) {
            completed = completed || self.completed;
            errorHandler = errorHandler || self.errorHandler;

            return $.ajax({
                url: url,
                data: postData,
                type: postData ? "POST" : self.method,
                processData: false, // always process on our own!
                contentType: self.contentType,
                timeout: self.timeout,
                dataType: "text",
                global: false,
                async: self.async,
                beforeSend: function beforeSend(xhr) {
                    for (var header in self.headers) xhr.setRequestHeader(header, self.headers[header]);
                    if (self.accepts)
                        xhr.setRequestHeader("Accept", self.accepts);
                },
                success: function success(result, status) {                    
                    var errorException = null;
                    if (self.evalResult) {
                        try {
                            result = JSON.parseWithDate(result);
                            if (result && result.hasOwnProperty("d"))
                                result = result.d;
                        } catch (e) {
                            errorException = new CallbackException(e);
                        }
                    }
                    if (errorException || result && (result.isCallbackError || result.iscallbackerror)) {
                        if (result)
                            errorException = result;
                        if (errorHandler)
                            errorHandler(errorException, self);
                        return;
                    }
                    if (completed)
                        completed(result, self);
                },
                error: function(xhr, status) {
                    var err = null;
                    if (xhr.readyState == 4) {
                        var res = xhr.responseText;
                        if (res && res.charAt(0) == '{')
                            err = JSON.parseWithDate(res);
                        if (!err) {
                            if (xhr.status && xhr.status != 200)
                                err = new CallbackException(xhr.status + " " + xhr.statusText);
                            else
                                err = new CallbackException("Callback Error: " + status);
                            err.detail = res;
                        }
                    }
                    if (!err)
                        err = new CallbackException("Callback Error: " + status);

                    if (errorHandler)
                        errorHandler(err, self, xhr);
                }
            });
        };
        this.returnError = function(message) {
            var error = new CallbackException(message);
            if (self.errorHandler)
                self.errorHandler(error, self);
        };
    };

    ServiceProxy = function(serviceUrl) {
        /// <summary>
        /// Generic Service Proxy class that can be used to
        /// call JSON Services generically using jQuery
        /// </summary>
        /// <param name="serviceUrl" type="string">The Url of the service ready to accept the method name</param>
        /// <example>
        /// var proxy = new ServiceProxy("JsonStockService.svc/");
        /// proxy.invoke("GetStockQuote",{symbol:"msft"},function(quote) { alert(result.LastPrice); },onPageError);
        ///</example>     
        var self = this;
        this.isWcf = true;
        this.timeout = 20000;
        this.method = "POST";
        this.serviceUrl = serviceUrl;

        if (typeof serviceUrl === "object")
            $.extend(this, serviceUrl);

        // Call a wrapped object
        this.invoke = function(method, params, callback, errorCallback, isBare) {
            /// <summary>
            /// Calls a WCF/ASMX service and returns the result.
            /// </summary> 
            /// <param name="method" type="string">The method of the service to call</param>
            /// <param name="params" type="object">An object that represents the parameters to pass {symbol:"msft",years:2}    
            /// <param name="callback" type="function">Function called on success. Receives a single parameter of the parsed result value</parm>
            /// <param name="errorCallback" type="function">Function called on failure. Receives a single error object with Message and StackDetail</parm>
            /// <param name="isBar" type="boolean">Set to true if response is not a WCF/ASMX style wrapped object</parm>

            // Convert input data into JSON using internal code
            var json = null;
            if (self.method != "GET")
                json = self.isWcf ? JSON.stringifyWcf(params) : JSON.stringify(params);

            // The service endpoint URL MyService.svc/    
            var url = self.serviceUrl + method;

            var http = new HttpClient({
                contentType: "application/json",
                accepts: "application/json,text/*",
                method: self.method,
                evalResult: true,
                timeout: self.timeout
            });
            http.send(url, json, callback, errorCallback);
        }
    };

    AjaxMethodCallback = function(controlId, url, opt) {
        var self = this;
        this.controlId = controlId;
        this.postbackMode = "PostMethodParametersOnly"; // Post,PostNoViewstate,Get
        this.serverUrl = url;
        this.formName = null;
        this.resultMode = "json"; // json,msajax,string
        this.timeout = 20000;

        this.completed = null;
        this.errorHandler = null;
        $.extend(this, opt);

        this.Http = null;

        this.callMethod = function(methodName, parameters, callback, errorCallback) {
            self.completed = callback;
            self.errorHandler = errorCallback;

            var http = new HttpClient({ timeout: self.timeout, evalResult: true, accepts: "application/json,text/*" });
            self.Http = http;

            var data = {};
            if (self.resultMode == "msajax")
                data = JSON.stringifyWithDates(parameters);
            else {
                var parmCount = 0;
                if (parameters.length) {
                    parmCount = parameters.length;
                    for (var x = 0; x < parmCount; x++) {
                        data["Parm" + (x + 1).toString()] = JSON.stringify(parameters[x]);
                    }
                }
                $.extend(data, {
                    CallbackMethod: methodName,
                    CallbackParmCount: parmCount,
                    __WWEVENTCALLBACK: self.controlId
                });

                data = $.param(data) + "&";
            }

            var formName = self.formName || (document.forms.length > 0 ? document.forms[0].id : "");

            if (self.postbackMode == "Post")
                data += $("#" + formName).serialize();
            else if (self.postbackMode == "PostNoViewstate")
                data += $("#" + formName).serializeNoViewState();
            else if (this.postbackMode == "Get") {
                Url = this.serverUrl;
                if (Url.indexOf('?') > -1)
                    Url += data;
                else
                    Url += "?" + data;

                return http.send(Url, null, self.onHttpCallback, self.onHttpCallback);
            }

            return http.send(this.serverUrl, data, self.onHttpCallback, self.onHttpCallback);
        };

        this.onHttpCallback = function(result) {
            if (result && (result.isCallbackError || result.iscallbackerror)) {
                if (self.errorHandler)
                    self.errorHandler(result, self);
                return;
            }
            if (self.completed != null)
                self.completed(result, self);
        };
    };
 
    ajaxJson = function(url, parm, cb, ecb, options) {
        var ser = parm;

        if (typeof cb === 'object') {
            options = cb;
            cb = null;
            ecb = null;
        }

        var verb = "POST";
        if (!parm)
            verb = "GET";

        var opt = {
            method: verb,
            contentType: "application/json",
            accepts: "application/json",
            noPostEncoding: false
        };
        $.extend(opt, options);

        var http = new HttpClient(opt);
        http.evalResult = true;        
        if (parm !== null && !opt.noPostEncoding && (opt.method === "POST" || opt.method === "PUT" || opt.method == "PATCH"))
            ser = JSON.stringify(parm);

        return http.send(url, ser, cb, ecb);
    };
    ajaxCallMethod = function(url, method, parms, cb, ecb, opt) {
        var proxy = new AjaxMethodCallback(null, url, opt);
        return proxy.callMethod(method, parms, cb, ecb);
    };
    $.postJSON = function(url, data, cb, ecb, opt) {
        var options = { method: "POST", evalResult: true };
        $.extend(options, opt);

        var http = new HttpClient(options);
        if (typeof data === "object")
            data = $.param(data);

        return http.send(url, data, cb, ecb);
    };
    $.fn.serializeObject = function() {
        var o = {};
        var a = this.serializeArray();
        $.each(a, function() {
            if (o[this.name] !== undefined) {
                if (!o[this.name].push)
                    o[this.name] = [o[this.name]];

                o[this.name].push(this.value || '');
            } else
                o[this.name] = this.value || '';
        });
        return o;
    };
    onPageError = function(err) {
        showStatus(err.message || err.Message, 6000, true);
    };
    CallbackException = function(message, detail, status) {
        this.isCallbackError = true;
        if (status)
            this.status = status;
        else
            this.status = 500;

        if (typeof message == "object") {
            if (message.message)
                this.message = message.message;
            else if (message.Message)
                this.message = message.Message;
        } else
            this.message = message;

        if (detail)
            this.detail = detail;
        else
            this.detail = null;
    };

    StatusBar = function(sel, opt) {
        var self = this;
        var _sb = null;

        // options  
        self.elementId = "_showstatus";
        self.prependMultiline = true;
        self.closable = false;
        self.afterTimeoutText = null;
        self.autoClose = false;
        self.noEffects = false;
        self.effectSpeed = 500;

        self.cssClass = "statusbar";
        self.highlightClass = "statusbarhighlight";
        self.closeButtonClass = "statusbarclose";
        self.additive = false;
        self.interval = 0;

        if (sel)
            _sb = $(sel);

        if (opt)
            $.extend(this, opt);

        // create statusbar object manually
        if (!_sb) {
            _sb = $("<div id='_statusbar' class='" + self.cssClass + "'>" +
                    "<div class='" + self.closeButtonClass + "'>" +
                    (self.closable ? "</div></div>" : ""))
                .appendTo(document.body)
                .hide();
        }

        if (self.closeable)
            $("." + self.cssClass).click(function(e) {
                self.hide();
            });

        this.show = function(message, timeout, isHighlighted, additive) {
            if (message == "hide")
                return self.hide();

            if (isHighlighted === true)
                _sb.addClass(self.highlightClass);
            else
                _sb.removeClass(self.highlightClass);

            if (self.additive) {
                var html = $("<div style='margin-bottom: 2px;'>" + message + "</div>");
                if (self.prependMultiline)
                    _sb.prepend(html);
                else
                    _sb.append(html);
            } else {
                if (!self.closable)
                    _sb.text(message);
                else {
                    var t = _sb.find("div.statusbarclose");
                    _sb.text(message).prepend(t);
                    t.click(self.hide);
                }
            }

            if (_sb.is(":visible") || self.noEffects)
                _sb.show();
            else
                _sb.slideDown(self.effectSpeed);
            _sb.maxZIndex();

            if (timeout) {
                if (self.interval != 0)
                    clearInterval(self.interval);

                self.interval = setTimeout(function() {
                    self.interval = 0;
                    _sb.removeClass(self.highlightClass);

                    if (self.afterTimeoutText)
                        self.show(self.afterTimeoutText);
                    else if
                    (self.autoClose)
                        self.hide();
                }, timeout);
            }
            return self;
        };

        this.hide = function() {
            if (self.noEffects)
                _sb.hide();
            else
                _sb.slideUp(self.effectSpeed);

            _sb.removeClass(self.highlightClass);
            return self;
        };

        this.release = function() {
            if (_sb) {
                $(_sb).remove();
            }
        };
    };

    // use this as a global instance to customize constructor
    // or do nothing and get a default status bar
    __statusbar = null;
    showStatus = function(message, timeout, isHighlighted, additive) {
        if (typeof message == "object") {
            if (__statusbar)
                __statusbar.release();

            __statusbar = new StatusBar(null, message);
            return;
        }
        if (!__statusbar)
            __statusbar = new StatusBar();

        __statusbar.show(message, timeout, isHighlighted, additive);
    };

    $.fn.centerInClient = function(options) {
        /// <summary>Centers the selected items in the browser window. Takes into account scroll position.
        /// Ideally the selected set should only match a single element.
        /// </summary> 
        /// <param name="options" type="Object">
        /// Options map: forceAbsolute, container, completed
        /// </param>
        /// <returns type="jQuery" />
        var opt = {
            forceAbsolute: false,
            container: window, // selector of element to center in
            completed: null,
            centerOnceOnly: false,
            keepCentered: false // keep window centered as it's resized
        };
        $.extend(opt, options);

        return this.each(function(i) {
            var el = $(this);

            // if centerOnceOnly is set center only once
            if (opt.centerOnceOnly) {
                if (el.data("_centerOnce"))
                    return;
                el.data("_centerOnce", true);
            } else
                el.data("_centerOnce", null);

            if (opt.keepCentered) {
                if (!el.data("_keepCentered")) {
                    el.data("_keepCentered", true);
                    $(window).resize(function() {
                        if (el.is(":visible"))
                            setTimeout(function() {
                                el.centerInClient(opt);
                            });
                    });
                }
            }

            var jWin = $(opt.container);
            var isWin = opt.container == window;

            // force to the top of document to ENSURE that
            // document absolute positioning is available
            if (opt.forceAbsolute) {
                if (isWin)
                    el.remove().appendTo("body");
                else
                    el.remove().appendTo(jWin[0]);
            }

            // have to make absolute
            el.css("position", "absolute");

            // height is off a bit so fudge it
            var heightFudge = 2.2;

            var x = (isWin ? jWin.width() : jWin.outerWidth()) / 2 - el.outerWidth() / 2;
            var y = (isWin ? jWin.height() : jWin.outerHeight()) / heightFudge - el.outerHeight() / 2;

            x = x + jWin.scrollLeft();
            y = y + jWin.scrollTop();
            y = y < 5 ? 5 : y;
            x = x < 5 ? 5 : x;
            el.css({ left: x, top: y });

            var zi = el.css("zIndex");
            if (!zi || zi == "auto")
                el.css("zIndex", 1);

            // if specified make callback and pass element
            if (opt.completed)
                opt.completed(this);
        });
    };

    // sums up CSS property values
    sumDimensions = function($el, dims) {
        // Opera returns -1 for missing min/max width, turn into 0
        var sum = 0;
        for (var i = 1; i < arguments.length; i++)
            sum += Math.max(parseInt($el.css(arguments[i]), 10) || 0, 0);
        return sum;
    };
    debounce = function(func, wait, immediate) {
        var timeout;
        return function() {
            var context = this, args = arguments;
            var later = function() {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow)
                func.apply(context, args);
        };
    };

    $.fn.makeAbsolute = function(rebase) {
        /// <summary>
        /// Makes an element absolute
        /// </summary> 
        /// <param name="rebase" type="boolean">forces element onto the body tag. Note: might effect rendering or references</param> 
        /// </param> 
        /// <returns type="jQuery" />
        return this.each(function() {
            var el = $(this);

            var isvis = true;
            if (!el.is(":visible")) {
                el.show();
                isvis = false;
            }
            var pos = el.position();
            if (!isvis)
                el.hide();

            el.css({
                position: "absolute",
                marginLeft: 0,
                marginTop: 0,
                top: pos.top,
                left: pos.left
            });
            if (rebase)
                el.remove().appendTo("body");
        });
    };
    $.fn.slideUpTransition = function(opt) {
        /// <summary>
        /// Like .slideUp() but uses transitions.
        /// Requires:
        /// 1 .Your element must be wrapped into a container object
        /// with no padding or margins (ie. add one!)
        /// 2. The container has to apply one or two styles for
        /// for each state.
        /// Styles:
        /// More info see:
        /// http://weblog.west-wind.com/posts/2014/Feb/22/Using-CSS-Transitions-to-SlideUp-and-SlideDown
        /// </summary>         
        /// <returns type="jQuery" />
        opt = $.extend(opt, {
            cssHiddenClass: "height-transition-hidden"
        });

        return this.each(function() {
            var $el = $(this);
            $el.css("max-height", "0");
            $el.addClass(opt.cssHiddenClass);
        });
    };

    $.fn.slideDownTransition = function(opt) {
        /// <summary>
        /// Like .slideDown() but uses transitions.
        /// Requires:
        /// 1 .Your element must be wrapped into a container object
        /// with no padding or margins (ie. add one!)
        /// 2. The container has to apply one or two styles for
        /// for each state.
        /// Styles:
        /// More info see:
        /// http://weblog.west-wind.com/posts/2014/Feb/22/Using-CSS-Transitions-to-SlideUp-and-SlideDown
        /// </summary>         
        /// <returns type="jQuery" />      
        opt = $.extend(opt, {
            cssHiddenClass: "height-transition-hidden"
        });

        return this.each(function() {
            var $el = $(this);
            $el.removeClass(opt.cssHiddenClass);

            // temporarily make visible to get the size
            $el.css("max-height", "none");
            var height = $el.outerHeight();

            // reset to 0 then animate with small delay
            $el.css("max-height", "0");

            setTimeout(function() {
                $el.css({ "max-height": height });
            }, 1);
        });
    };

    $.fn.stretchToBottom = function(options) {
        /// <summary>
        /// Stretches an element to the bottom of another element like window
        /// to provide 100% bottom fill to simulate height: 100%.
        /// </summary> 
        /// <param name="options" type="object">
        /// 1) jQuery Selector of the Container
        /// 2) Options map:
        ///      container: jQuery selector for container element
        ///      autoResize: when true resizes as window resizes
        ///      bottomOffset: manual override for offset from the bottom
        /// </param> 
        /// <returns type="jQuery" />
        var opt = {
            container: $(window),
            bottomOffset: 0,
            autoResize: false
        };

        if (options && options.length)
            opt.container = options;
        else
            $.extend(opt, options);

        if (opt.autoResize == true) {
            $els = this;
            $(opt.container).resize(function() {
                $els.stretchToBottom({ container: opt.container, autoResize: false });
            });
        }

        return this.each(function() {
            $el = $(this);
            var oabs = $el.css("position");
            $el.makeAbsolute();
            var $cont = opt.container;

            var bott = $(window).innerHeight();
            var top = parseInt($el.css("top"));
            var height = 0;
            if ($cont[0] != window) {
                var ds = sumDimensions($cont, "borderTopWidth", "borderBottomWidth", "paddingBottom", "paddingTop") +
                    sumDimensions($el, "borderTopWidth", "borderBottomWidth", "marginBottom", "marginTop", "paddingBottom", "paddingTop");
                ds = ds ? ds : 1;
                bott = $cont.offset().top + $cont.outerHeight();
                height = bott - top - Math.ceil(ds) - opt.bottomOffset;
                //console.log("*id: " + this.id + "  bott: " + bott + " Top: " + top + " - " + $cont.offset().top  + " ds: " + ds + "  offset: " + opt.bottomOffset + " height: " + height + " cont height:" + $cont.innerHeight() + " " + $cont.outerHeight());
            } else {
                var ds = sumDimensions($el, "borderTopWidth", "borderBottomWidth", "marginBottom", "marginTop");
                height = bott - top - Math.ceil(ds) - opt.bottomOffset;
                //console.log("id: " + this.id + "  bott: " + bott + " Top: " + top + " - " + $cont.offset().top + " ds: " + ds + "  offset: " + opt.bottomOffset + " height: " + height);
            }

            $el.css("position", oabs).css("height", height);
        });
    };

    $.fn.moveToMousePosition = function(evt, options) {
        var opt = { left: 0, top: 0 };
        $.extend(opt, options);
        return this.each(function() {
            var el = $(this);
            el.css({
                left: evt.pageX + opt.left,
                top: evt.pageY + opt.top,
                position: "absolute"
            });
        });
    };


    $.fn.tooltip = function(msg, timeout, options) {
        var opt = {
            cssClass: "tooltip",
            isHtml: false,
            onRelease: null
        };
        $.extend(opt, options);

        return this.each(function() {
            var tp = new _ToolTip(this, opt);
            if (msg == "hide") {
                tp.hide();
                return;
            }
            tp.show(msg, timeout, opt.isHtml);
        });

        function _ToolTip(sel, opt) {
            var _I = this;
            var jEl = $(sel);

            this.cssClass = "";
            this.onRelease = null;
            $.extend(_I, opt);

            var el = jEl.get(0);
            var tt = $("#" + el.id + "_tt");

            this.show = function(msg, timeout, isHtml) {
                if (tt.length > 0)
                    tt.remove();

                tt = $("<div>").attr("id", el.id + "_tt");

                $(document.body).append(tt);

                tt.css({
                    position: "absolute",
                    display: "none",
                    zIndex: 1000
                });
                if (_I.cssClass)
                    tt.addClass(_I.cssClass);
                else
                    tt.css({
                        background: "cornsilk",
                        border: "solid 1px gray",
                        fontSize: "8pt",
                        padding: 2,
                        "border-radius": "2px",
                        "box-shadow": "1px 1px 1px #535353"
                    });

                if (isHtml)
                    tt.html(msg);
                else
                    tt.text(msg);

                var pos = jEl.position();

                var Left = pos.left + 5;
                var Top = pos.top + jEl.outerHeight() - 1;

                var Width = tt.width();
                if (Width > 400)
                    Width = 400;

                tt.css({
                    left: Left,
                    top: Top,
                    width: Width
                });
                tt.show();

                if (timeout && timeout > 0)
                    setTimeout(function() {
                        if (_I.onRelease)
                            _I.onRelease.call(el, _I);
                        _I.hide();
                    }, timeout);
            };
            this.hide = function() {
                if (tt.length > 0)
                    tt.fadeOut("slow");
            };
        }
    };

    $.fn.watch = function(options) {
        /// <summary>
        /// Allows you to monitor changes in a specific
        /// CSS property of an element by polling the value.
        /// You can also monitor attributes (using attr_ prefix)
        /// or property changes (using prop_ prefix).
        /// when the value changes a function is called.
        /// The callback is fired in the context
        /// of the selected element (ie. this)
        ///
        /// Uses the MutationObserver API of the DOM and
        /// falls back to setInterval to poll for changes
        /// for non-compliant browsers (pre IE 11)
        /// </summary>            
        /// <param name="options" type="Object">
        /// Option to set - see comments in code below.
        /// </param>        
        /// <returns type="jQuery" /> 

        var opt = $.extend({
            // CSS styles or Attributes to monitor as comma delimited list
            // For attributes use a attr_ prefix
            // Example: "top,left,opacity,attr_class"
            properties: null,

            // interval for 'manual polling' (IE 10 and older)            
            interval: 100,

            // a unique id for this watcher instance
            id: "_watcher_" + new Date().getTime(),

            // flag to determine whether child elements are watched            
            watchChildren: false,

            // Callback function if not passed in callback parameter   
            callback: null
        }, options);

        return this.each(function() {
            var el = this;
            var el$ = $(this);
            var fnc = function(mRec, mObs) {
                __watcher.call(el, opt.id, mRec, mObs);
            };

            var data = {
                id: opt.id,
                props: opt.properties.split(','),
                vals: [opt.properties.split(',').length],
                func: opt.callback, // user function
                fnc: fnc, // __watcher internal
                origProps: opt.properties,
                interval: opt.interval,
                intervalId: null
            };
            // store initial props and values
            $.each(data.props, function(i) {
                var propName = data.props[i];
                if (data.props[i].startsWith('attr_'))
                    data.vals[i] = el$.attr(propName.replace('attr_', ''));
                else if (propName.startsWith('prop_'))
                    data.vals[i] = el$.prop(propName.replace('props_', ''));
                else
                    data.vals[i] = el$.css(propName);
            });

            el$.data(opt.id, data);

            hookChange(el$, opt.id, data);
        });

        function hookChange(element$, id, data) {
            element$.each(function() {
                var el$ = $(this);

                if (window.MutationObserver) {
                    var observer = el$.data('__watcherObserver' + opt.id);
                    if (observer == null) {
                        observer = new MutationObserver(data.fnc);
                        el$.data('__watcherObserver' + opt.id, observer);
                    }
                    observer.observe(this, {
                        attributes: true,
                        subtree: opt.watchChildren,
                        childList: opt.watchChildren,
                        characterData: true
                    });
                } else
                    data.intervalId = setInterval(data.fnc, opt.interval);
            });
        }

        function __watcher(id, mRec, mObs) {
            var el$ = $(this);
            var w = el$.data(id);
            if (!w) return;
            var el = this;

            if (!w.func)
                return;

            var changed = false;
            var i = 0;
            for (i; i < w.props.length; i++) {
                var key = w.props[i];

                var newVal = "";
                if (key.startsWith('attr_'))
                    newVal = el$.attr(key.replace('attr_', ''));
                else if (key.startsWith('prop_'))
                    newVal = el$.prop(key.replace('prop_', ''));
                else
                    newVal = el$.css(key);

                if (newVal == undefined)
                    continue;

                if (w.vals[i] !== newVal) {
                    w.vals[i] = newVal;
                    changed = true;
                    break;
                }
            }
            if (changed) {
                // unbind to avoid recursive events
                el$.unwatch(id);

                // call the user handler
                w.func.call(el, w, i, mRec, mObs);

                // rebind the events
                hookChange(el$, id, w);
            }
        }
    };

    $.fn.unwatch = function(id) {
        this.each(function() {
            var el = $(this);
            var data = el.data(id);
            try {
                if (window.MutationObserver) {
                    var observer = el.data("__watcherObserver" + id);
                    if (observer) {
                        observer.disconnect();
                        el.removeData("__watcherObserver" + id);
                    }
                } else
                    clearInterval(data.intervalId);
            }
            // ignore if element was already unbound
            catch (e) {
            }
        });
        return this;
    };


    $.fn.listSetData = function (items, options) {
        var opt = {
            noClear: false, // don't clear the list first if true
            dataValueField: null, // optional value field for object lists
            dataTextField: null
        };
        $.extend(opt, options);

        return this.each(function () {
            var el = $(this);

            if (items == null) {
                el.children().remove();
                return;
            }
            if (!opt.noClear) el.children().remove();

            if (items.Rows)
                items = items.Rows;
            else if (items.rows)
                items = items.rows;

            var isValueList = false;

            if (!opt.dataTextField && !opt.dataValueField)
                isValueList = true;

            for (x = 0; x < items.length; x++) {
                var row = items[x];
                if (isValueList)
                    el.listAddItem(row, row);
                else
                    el.listAddItem(row[opt.dataTextField], row[opt.dataValueField]);
            }
        });
    };
    $.fn.listAddItem = function (text, value) {
        return this.each(function () {
            $(this).append($("<option></option>")
                   .attr("value", value)
                   .text(text));
        });
    };
    $.fn.listSelectItem = function (value) {
        if (this.length < 1)
            return;
        var list = this.get(0);
        if (!list.options)
            return;

        for (var x = list.options.length - 1; x > -1; x--) {
            if (list.options[x].value === value) {
                list.options[x].selected = true;
                return;
            }
        }
        return this;
    };
    $.fn.listGetSelections = function (singleValue) {
        var opts = this.find("option:selected");
        if (singleValue)
            if (opts.length > 0)
                return sels.eq(0).val();
            else
                return null;
        var sels = [];
        for (var i = 0; i < opts.length; i++) {
            sels.push(opts.eq(i).val());
        }
        return sels;
    };

    HoverPanel = function (sel, opt) {
        var _I = this;
        var jEl = $(sel);
        var el = jEl.get(0);

        var busy = -1;
        var lastMouseTop = 0;
        var lastMouseLeft = 0;

        this.serverUrl = "";
        this.timeout = 20000;
        this.controlId = el.id;
        this.htmlTargetId = el.id;
        this.queryString = "";
        this.eventHandlerMode = "ShowHtmlAtMousePosition";
        this.postbackMode = "Get"; // Post/PostNoViewstate
        this.completed = null;
        this.errorHandler = null;
        this.hoverOffsetRight = 0;
        this.hoverOffsetBottom = 0;
        this.panelOpacity = 1;
        this.adjustWindowPosition = true;
        this.formName = "";
        this.navigateDelay = 0;
        this.http = null;
        $.extend(_I, opt);

        this.startCallback = function (e, queryString, postData, errorHandler) {
            try {
                var key = new Date().getTime();
                _I.busy = key;
                var url = this.serverUrl;
                if (e) {
                    _I.lastMouseTop = e.clientY;
                    _I.lastMouseLeft = e.clientX;
                } else
                    _I.lastMouseTop = 0;

                if (queryString == null)
                    _I.queryString = queryString = "";
                else
                    _I.queryString = queryString;

                if (errorHandler)
                    _I.errorHandler = errorHandler;

                if (queryString)
                    queryString += "&";
                else queryString = "";
                queryString += "__WWEVENTCALLBACK=" + _I.controlId;

                _I.formName = _I.formName || document.forms[0];

                _I.http = new HttpClient();
                _I.timeout = _I.timeout;
                _I.http.appendHeader("RequestKey", key);

                if (postData)
                    postData += "&";
                else
                    postData = "";

                if (_I.postbackMode == "Post")
                    postData += $(_I.formName).serialize();
                else if (this.postbackMode == "PostNoViewstate")
                    postData += $(_I.formName).serializeNoViewState();
                else if (this.postbackMode == "Get" && postData)
                    queryString += postData;

                if (queryString != "") {
                    if (url.indexOf("?") > -1)
                        url = url + "&" + queryString;
                    else
                        url = url + "?" + queryString;
                }

                if (_I.eventHandlerMode == 'ShowIFrameAtMousePosition' || _I.eventHandlerMode == 'ShowIFrameInPanel') {
                    setTimeout(function () {
                        if (_I.busy) _I.showIFrame.call(_I, url);
                    }, _I.navigateDelay);
                    return;
                }

                // Send the request with navigate delay
                setTimeout(function () {
                    if (_I.busy === key)
                        _I.http.send.call(_I, url, postData, _I.onHttpCallback, _I.onHttpCallback);
                }, _I.navigateDelay);
            } catch (e) {
                // Call with 'error message'
                _I.onHttpCallback(new CallbackException(e.message));
            }
        };
        this.onHttpCallback = function (result) {
            _I.busy = -1;

            if (_I.http && _I.http.status && _I.http.status != 200)
                result = new CallbackException(http.statusText);
            if (result == null)
                result = new CallbackException("No output was returned.");

            if (result.isCallbackError) {
                if (_I.errorHandler)
                    _I.errorHandler(result);
                return;
            }
            _I.displayResult(result);
        };
        this.displayResult = function (result) {
            if (_I.completed && _I.completed(result, _I) == false)
                return;
            if (_I.eventHandlerMode == "ShowHtmlAtMousePosition") {
                _I.assignContent(result);
                _I.movePanelToPosition(_I.lastMouseLeft + _I.hoverOffsetRight, _I.lastMouseTop + _I.hoverOffsetBottom);
                _I.show();
            } else if (_I.eventHandlerMode == "ShowHtmlInPanel") {
                _I.assignContent(result);
                _I.show();
            }
        };
        this.assignContent = function (result) {
            $("#" + _I.htmlTargetId).html(result);
        };
        this.movePanelToPosition = function (x, y) {
            try {
                jEl.css("position", "absolute");

                if (typeof x == "object") {
                    _I.lastMouseTop = x.clientY;
                    _I.lastMouseLeft = x.clientX;
                } else if (typeof x == "number") {
                    _I.lastMouseTop = y;
                    _I.lastMouseLeft = x;
                }

                x = _I.lastMouseLeft + 3;
                y = _I.lastMouseTop + 3;
                var jWin = $(window);
                jEl.css({ left: x + jWin.scrollLeft(), top: y + jWin.scrollTop() });

                if (_I.adjustWindowPosition && document.body) {
                    var mainHeight = jWin.height();
                    var panHeight = jEl.outerHeight();
                    var mainWidth = jWin.width();
                    var panWidth = jEl.outerWidth();

                    if (mainHeight < panHeight)
                        y = 0;
                    else {
                        if (mainHeight < _I.lastMouseTop + panHeight)
                            y = mainHeight - panHeight - 10;
                    }

                    if (mainWidth < panWidth)
                        x = 0;
                    else {
                        if (mainWidth < _I.lastMouseLeft + panWidth)
                            x = mainWidth - panWidth - 25;
                    }
                    jEl.css({ left: x + jWin.scrollLeft(), top: y + jWin.scrollTop() });
                }
            } catch (e) {
                window.status = 'Moving of window failed: ' + e.message;
            }
        };
        this.showIFrame = function (Url) {
            _I.busy = false;
            Url = Url ? Url : _I.serverUrl;
            $("#" + _I.controlId + '_IFrame').attr("src", Url).load(_I.completed);
            _I.show();
            if (_I.eventHandlerMode == "ShowIFrameAtMousePosition")
                _I.movePanelToPosition(_I.lastMouseLeft + _I.hoverOffsetRight, _I.lastMouseTop + _I.hoverOffsetBottom);
        };
        this.hide = function () {
            this.abort();
            jEl.hide();
        };
        this.abort = function () {
            _I.busy = -1;
        };
        this.show = function () {
            jEl.show().css("opacity", _I.panelOpacity);
        };
    };

    _ModalDialog = function (sel, opt) {
        var _I = this;
        var jEl = $(sel);
        if (jEl.length < 1)
            jEl = $("#" + sel);
        if (jEl.length < 1)
            return;

        this.overlayId = "_ModalOverlay";

        this.contentId = jEl.get(0).id;
        this.headerId = "";
        this.backgroundOpacity = .75;
        this.fadeInBackground = false;
        this.zIndex = 0;
        this.jOverlay = null;
        this.keepCentered = true;
        this.dialogHandler = null;
        $.extend(_I, opt);
        var hideLists = null;

        this.show = function (msg, head, asHtml) {
            if (_I.contentId && typeof msg == "string")
                !asHtml ? $("#" + _I.contentId).text(msg) : $("#" + _I.contentId).html(msg);
            if (_I.headerId && typeof head == "string")
                !asHtml ? $("#" + _I.headerId).text(head) : $("#" + _I.headerId).html(head);

            _I.zIndex = _I.zIndex > 0 ? _I.zIndex : $.maxZIndex();
            jEl.css({ zIndex: _I.zIndex + 2 }).show().centerInClient();

            var bg = opaqueOverlay({ zIndex: _I.zIndex + 1, sel: "#" + _I.overlayId, opacity: _I.backgroundOpacity });
            _I.zIndex++;
            if (_I.fadeInBackground)
                bg.hide().fadeIn("slow");

            // track any clicks inside modal dialog
            jEl.click(_I.callback);

            if (_I.keepCentered)
                $(window).bind("resize.modal", function () {
                    jEl.centerInClient();
                }).bind("scroll.modal", function () {
                    jEl.centerInClient();
                });
        };
        this.hide = function () {
            jEl.hide();
            if (_I.keepCentered)
                $(window).unbind("resize.modal").unbind("scroll.modal");
            opaqueOverlay("hide", { sel: "#" + _I.overlayId });
            jEl.unbind("click");

            // restore IE list boxes
            if (hideLists) {
                hideLists.show();
                hideLists = null;
            }
        };
        this.callback = function (e) {
            // handle clicks only for buttons/links
            if ($(e.target).is(":button,a,.closebox")) {
                if (_I.dialogHandler) {
                    if (_I.dialogHandler.call(e.target, e, _I) == false)
                        return;
                    setTimeout(function () {
                        _I.hide();
                    }, 10);
                    return;
                }
                setTimeout(function () {
                    _I.hide();
                }, 10);
            }
        };
    };
    $.fn.modalDialog = function (opt, msg, head, asHtml, handler) {
        if (this.length < 1)
            return this;

        // only works with a single instance
        var el = this.get(0);
        var jEl = $(el);
        var dId = "modal" + el.id;

        var md = jEl.data(dId);
        if (!md)
            md = new _ModalDialog(jEl, opt);
        if (typeof opt == "string") {
            if (opt == "hide" || opt == "close")
                md.hide();
            if (opt == "instance" || opt == "get")
                return md;
            return;
        }
        md.show(msg, head, asHtml);
        jEl.data(dId, md);

        return this;
    };
    $.modalDialog = function (msg, header, aButtons, handler, isHtml) {
        var dl = $("#_MBOX");
        if (dl.length < 1) {
            dl = $("<div>").addClass("dialog dragwindow").attr("id", "_MBOX").css({ width: 400 });
            var head = $("<div>").addClass("dialog-header").attr("id", "_MBOXHEADER");
            var ctn = $("<div>").addClass("dialog-content").attr("id", "_MBOXCONTENT");

            dl.append(head).append(ctn);
            var btns = $("<div>").css("margin", "0px 15px 15px");
            if (!aButtons)
                aButtons = [" Close "];
            for (var i = 0; i < aButtons.length; i++) {
                var btn = $("<input type='button' />").attr("id", "_BTN_" + i).css("margin-right", "5px").val(aButtons[i]);
                btns.append(btn);
            }
            dl.append(btns).appendTo(document.body);
        }
        if (!handler)
            handler = function () {
                if (this.id.substr(0, 5) == "_BTN_" || $(this).hasClass("closebox")) return true;
                return false;
            };
        dl.modalDialog({ dialogHandler: handler, headerId: "_MBOXHEADER", contentId: "_MBOXCONTENT" },
	                 msg, header, isHtml)
	 .draggable({ handle: $("#_MBOX .dialog-header") })
	 .closable({
	     closeHandler: function closeHandler() {
	         var close = true;
	         if (handler)
	             close = handler.call(this);
	         if (close)
	             $("#_MBOX").modalDialog("hide");
	     }
	 });
    };
    opaqueOverlay = function (opt, p2) {
        var _I = this;
        var jWin = $(window);

        this.sel = "#_ShadowOverlay";
        this.opacity = 0.75;
        this.zIndex = 10000;
        $.extend(this, p2 || opt);

        var sh = $(sel);
        if (opt == "hide") {
            if (sh.length < 1)
                return;
            sh.hide();
            sh.get(0).opaqueOverlay = false;
            jWin.unbind("resize.opaque").unbind("scroll.opaque");
            return;
        }

        if (sh.length < 1)
            sh = $("<div>").attr("id", this.sel.substr(1))
                           .css("background", "black")
                       .appendTo(document.body);

        var el = sh.get(0);
        sh.show();

        if (!el.opaqueOverlay)
            jWin.bind("resize.opaque", function () {
                opaqueOverlay(opt);
            }).bind("scroll.opaque", function () {
                opaqueOverlay(opt);
            });

        el.opaqueOverlay = true;

        sh.css({ top: 0 + jWin.scrollTop(), left: 0 + jWin.scrollLeft(), position: "absolute", opacity: _I.opacity, zIndex: _I.zIndex })
	  .width(jWin.width())
	  .height(jWin.height());

        return sh;
    };

    if (!$.fn.draggable) {
        $.fn.draggable = function (opt) {
            return this.each(function () {
                var el = $(this);
                var drag = el.data("draggable");

                if (typeof opt == "string") {
                    if (drag && opt == "remove") {
                        drag.stopDragging();
                        el.removeData("draggable");
                    }
                    return;
                }
                if (!drag) {
                    drag = new DragBehavior(this, opt);
                    el.data("draggable", drag);
                }
            });
        };
        var __dragIndex = 1;

        DragBehavior = function (sel, opt) {
            var _I = this;
            var el = $(sel);

            this.handle = "";
            this.opacity = 0.75;
            this.start = null;
            this.stop = null;
            this.dragDelay = 100;
            this.forceAbsolute = false;

            $.extend(_I, opt);

            _I.handle = _I.handle ? $(_I.handle, el) : el;
            if (_I.handle.length < 1)
                _I.handle = el;

            var isMouseDown = false;
            var isDrag = false;
            var timeMD = 0;
            var clicked = -1;
            var deltaX = 0;
            var deltaY = 0;
            var savedOpacity = 1;
            var savedzIndex = 0;

            this.mouseDown = function (e) {
                var dEl = _I.handle.get(0);
                var s = false;
                $(e.target).parents().each(function () {
                    if (this == dEl) s = true;
                });
                if (isMouseDown || e.target != dEl && !s || $(e.target).is(".closebox,input,textara,a")) return;

                isMouseDown = true;
                isDrag = false;

                var pos = _I.handle.offset();
                deltaX = e.pageX - pos.left;
                deltaY = e.pageY - pos.top;

                setTimeout(function () {
                    if (!isMouseDown) return;
                    el.show().makeAbsolute(_I.forceAbsolute);
                    _I.dragActivate(e);
                }, _I.dragDelay);
            };
            var nf = function nf(e) {
                e.stopPropagation();
                e.preventDefault();
            };
            this.dragActivate = function (e) {
                if (!isMouseDown) return;
                isDrag = true;
                _I.moveToMouse(e);
                isMouseDown = true;
                savedzIndex = el.css("zIndex");
                el.css("zIndex", 150000);
                savedOpacity = el.css("opacity");
                el.css({ opacity: _I.opacity, cursor: "move" });

                $(document).bind("mousemove.dbh", _I.mouseMove);
                $(document).bind("selectstart.dbh", nf);
                $(document).bind("dragstart.dbh", nf);
                $(document.body).bind("dragstart.dbh", nf);
                $(document.body).bind("selectstart.dbh", nf);
                _I.handle.bind("selectstart.dbh", nf);
                if (_I.start)
                    _I.start(e, _I);
            };
            this.dragDeactivate = function (e, noMove) {
                if (!isMouseDown) return;
                isMouseDown = false;

                if (!isDrag) return;
                isDrag = false;

                if (!noMove)
                    _I.moveToMouse(e);
                $(document).unbind("mousemove.dbh");

                $(document).unbind("selectstart.dbh");
                $(document).unbind("dragstart.dbh");
                $(document.body).unbind("dragstart.dbh");
                $(document.body).unbind("selectstart.dbh");
                _I.handle.unbind("selectstart.dbh");

                if (!noMove) {
                    __dragIndex += 10;
                    el.css({ zIndex: 10000 + __dragIndex, cursor: "auto" });
                    el.css("opacity", savedOpacity);
                    if (_I.stop)
                        _I.stop(e, _I);
                }
            };
            this.mouseUp = function (e) {
                _I.dragDeactivate(e);
            };
            this.mouseMove = function (e) {
                if (isMouseDown) _I.moveToMouse(e);
            };
            this.moveToMouse = function (e) {
                el.css({ left: e.pageX - deltaX, top: e.pageY - deltaY });
            };
            this.stopDragging = function () {
                if (!isDrag) return;
                _I.dragDeactivate(null, true);
                $(document).unbind("mousedown", _I.mouseDown);
            };
            $(document).mousedown(_I.mouseDown);
            $(document).mouseup(_I.mouseUp);
        };
    }

    if (!$.fn.resizable) {
        $.fn.resizable = function fnResizable(options) {
            var opt = {
                // selector for handle that starts dragging
                handleSelector: null,
                // resize the width
                resizeWidth: true,
                // resize the height
                resizeHeight: true,
                // hook into start drag operation (event passed)
                onDragStart: null,
                // hook into stop drag operation (event passed)
                onDragEnd: null,
                // hook into each drag operation (event passed)
                onDrag: null,
                // disable touch-action on $handle
                // prevents browser level actions like forward back gestures
                touchActionNone: true
            };
            if (typeof options == "object") opt = $.extend(opt, options);

            return this.each(function () {
                var startPos, startTransition;

                var $el = $(this);
                var $handle = opt.handleSelector ? $(opt.handleSelector) : $el;

                if (opt.touchActionNone)
                    $handle.css("touch-action", "none");

                $el.addClass("resizable");
                $handle.bind('mousedown.rsz touchstart.rsz', startDragging);

                function noop(e) {
                    e.stopPropagation();
                    e.preventDefault();
                };

                function startDragging(e) {
                    startPos = getMousePos(e);
                    startPos.width = parseInt($el.width(), 10);
                    startPos.height = parseInt($el.height(), 10);

                    startTransition = $el.css("transition");
                    $el.css("transition", "none");

                    if (opt.onDragStart) {
                        if (opt.onDragStart(e, $el, opt) === false)
                            return;
                    }
                    opt.dragFunc = debounce(doDrag,20);

                    $(document).bind('mousemove.rsz', opt.dragFunc);
                    $(document).bind('mouseup.rsz', stopDragging);
                    if (window.Touch || navigator.maxTouchPoints) {
                        $(document).bind('touchmove.rsz', opt.dragFunc);
                        $(document).bind('touchend.rsz', stopDragging);
                    }
                    $(document).bind('selectstart.rsz', noop); // disable selection
                }

                function doDrag(e) {
                    var pos = getMousePos(e);

                    if (opt.resizeWidth) {
                        var newWidth = startPos.width + pos.x - startPos.x;
                        $el.width(newWidth);
                    }

                    if (opt.resizeHeight) {
                        var newHeight = startPos.height + pos.y - startPos.y;
                        $el.height(newHeight);
                    }

                    if (opt.onDrag)
                        opt.onDrag(e, $el, opt);
                    
                }

                function stopDragging(e) {
                    e.stopPropagation();
                    e.preventDefault();

                    $(document).unbind('mousemove.rsz', opt.dragFunc);
                    $(document).unbind('mouseup.rsz', stopDragging);

                    if (window.Touch || navigator.maxTouchPoints) {
                        $(document).unbind('touchmove.rsz', opt.dragFunc);
                        $(document).unbind('touchend.rsz', stopDragging);
                    }
                    $(document).unbind('selectstart.rsz', noop);

                    // reset changed values
                    $el.css("transition", startTransition);

                    if (opt.onDragEnd)
                        opt.onDragEnd(e, $el, opt);

                    return false;
                }

                function getMousePos(e) {
                    var pos = { x: 0, y: 0, width: 0, height: 0 };
                    if (typeof e.clientX === "number") {
                        pos.x = e.clientX;
                        pos.y = e.clientY;
                    } else if (e.originalEvent.touches) {
                        pos.x = e.originalEvent.touches[0].clientX;
                        pos.y = e.originalEvent.touches[0].clientY;
                    } else
                        return null;

                    return pos;
                }
            });
        };
    }

    $.fn.closable = function (options) {
        var opt = {
            handle: null,
            closeHandler: null,
            cssClass: "closebox", // closebox-container
            imageUrl: null,
            fadeOut: null
        };
        opt = $.extend(opt, options);

        return this.each(function (i) {
            var el = $(this);
            var pos = el.css("position");
            if (!pos || pos == "static")
                el.css("position", "relative");

            var h = opt.handle ? $(opt.handle, el).css({ position: "relative" }) : el;

            var div = el.find("." + opt.cssClass);
            var exists = true;
            if (div.length < 1) {
                div = opt.imageUrl ? $("<img />").attr("src", opt.imageUrl).css("cursor", "pointer") : $("<div></div>");
                div.addClass(opt.cssClass);
                exists = false;
            }
            div.click(function (e) {
                if (opt.closeHandler && !opt.closeHandler.call(this, e))
                    return;
                if (opt.fadeOut)
                    $(el).fadeOut(opt.fadeOut);
                else
                    $(el).hide();
            });
            if (opt.imageUrl) div.css("background-image", "none");

            if (!exists)
                h.append(div);
        });
    };

    $.fn.contentEditable = function (opt) {
        if (this.length < 1)
            return;
        var oldPadding = "0px";
        var def = {
            editClass: null,
            saveText: "Save",
            saveHandler: null
        };
        $.extend(def, opt);

        this.each(function () {
            var jContent = $(this);

            if (this.contentEditable == "true")
                return this; // already editing

            var jButton = $("<input type='button' value='" + def.saveText + "' class='editablebutton' style='display: block;'/>");

            var cleanupEditor = function cleanupEditor() {
                if (def.editClass)
                    jContent.removeClass(def.editClass);
                else
                    jContent.css({ background: "transparent", padding: oldPadding });
                jContent.get(0).contentEditable = false;
                jButton.remove();
            };

            jButton.click(function (e) {
                if (def.saveHandler.call(jContent.get(0), e))
                    cleanupEditor();
            });
            jContent.keypress(function (e) {
                if (e.keyCode == 27)
                    cleanupEditor();
            });

            jContent
            	.after(jButton)
            	.css("margin", 2);

            this.contentEditable = true;

            if (def.editClass)
                jContent.addClass(def.editClass);
            else {
                oldPadding = jContent.css("padding");
                jContent.css({ background: "lavender", padding: 10 });
            }
            return this;
        });
        return this;
    };
    $.fn.editable = function (opt) {
        if (this.length < 1) return this;

        var oldPadding = "0px";
        var def = {
            editClass: null,
            saveText: "Save",
            editMode: "text", // html
            saveHandler: null,
            value: null
        };
        $.extend(def, opt);

        this.each(function () {
            var jContent = $(this);

            if (opt == "cleanup") {
                jContent.data("cleanupEditor")();
                return this;
            }

            if (jContent.data("editing"))
                return this;

            var jButton = $("<input type='button' />")
                .addClass("editablebutton")
                .css({ display: "block" })
                .val(def.saveText);
            var jEdit = $("<textarea id='_contenteditor'></textarea>")
                .css({ fontFamily: jContent.css("font-family"), minHeight: "18px" });
            if (def.value) jEdit.val(def.value);
            else jEdit.val(def.editMode == "text" ? jContent.text() : jContext.html());
            if (def.editClass)
                jEdit.addClass(def.editClass);
            else
                jEdit.width(jContent.width() - 10)
                    .height(jContent.height());

            jEdit.focus()
                .hide()
                .fadeIn("slow")
                .data("editing", jContent.get(0))
                .insertBefore(jContent)
                .keypress(function (e) {
                    if (e.keyCode == 27)
                        cleanupEditor();
                });

            jContent.data("editing", true).hide();

            jContent.data("cleanupEditor", function () {
                jEdit.remove();
                jButton.remove();
                jContent.data("editing", false)
                    .data("cleanupEditor", null)
                    .fadeIn("slow");
            });

            jButton.click(function (e) {
                var pass = {
                    text: jEdit.val(),
                    cleanup: jContent.data("cleanupEditor"),
                    button: jButton,
                    edit: jEdit,
                    content: jContent
                };
                if (def.saveHandler.call(jEdit.get(0), pass))
                    cleanupEditor();
            });

            jEdit
                .after(jButton)
                .css("margin", 2);
            return this;
        });
        return this;
    };

    $.maxZIndex = $.fn.maxZIndex = function (opt) {
        /// <summary>
        /// Returns the max zOrder in the document (no parameter)
        /// Sets max zOrder by passing a non-zero number
        /// which gets added to the highest zOrder.
        /// </summary> 
        /// <param name="opt" type="object">
        /// inc: increment value,
        /// group: selector for zIndex elements to find max for
        /// </param>
        /// <returns type="jQuery" />
        var def = { inc: 10, group: "*" };
        $.extend(def, opt);
        var zmax = 0;
        $(def.group).each(function () {
            var cur = parseInt($(this).css('z-index'));
            zmax = cur > zmax ? cur : zmax;
        });
        if (!this.jquery)
            return zmax;

        return this.each(function () {
            zmax += def.inc;
            $(this).css("z-index", zmax);
        });
    };

    /// <script type="text/html" id="script">
    /// <div>
    ///   <#= content #>
    ///   <# for(var i=0; i < names.length; i++) { #>
    ///   Name: <#= names[i] #> <br/>
    ///   <# } #>
    /// </div>
    /// </script>
    ///
    /// var tmpl = $("#itemtemplate").html();
    /// var data = { content: "This is some textual content",
    ///              names: ["rick", "markus"]
    /// };
    /// $("#divResult").html(parseTemplate(tmpl, data));
    ///
    /// based on John Resig's Micro Templating engine
    var _tmplCache = {};
    parseTemplate = function (str, data) {
        /// <summary>
        /// Client side template parser that uses &lt;#= #&gt; and &lt;# code #&gt; expressions.
        /// and # # code blocks for template expansion.
        /// </summary> 
        /// <param name="str" type="string">The text of the template to expand</param> 
        /// <param name="data" type="var">
        /// Any data that is to be merged. Pass an object and
        /// that object's properties are visible as variables.
        /// </param> 
        /// <returns type="string" />
        var err = "";

        try {
            var func = _tmplCache[str];
            if (!func) {
                var strFunc =
            "var p=[];with(obj){p.push('" +
            str.replace(/[\r\t\n]/g, " ")
               .replace(/'(?=[^#]*#>)/g, "\t")
               .split("'").join("\\'")
               .split("\t").join("'")
               .replace(/<#=(.+?)#>/g, "',$1,'")
               .split("<#").join("');")
               .split("#>").join("p.push('")
               + "');}return p.join('');";
                func = new Function("obj", strFunc);
                _tmplCache[str] = func;
            }

            return func.call(data, data);
        } catch (e) {
            err = e.message;
        }
        return "< # ERROR: " + err.htmlEncode() + " # >";
    };

    isElementInViewport = function (el) {
        var rect = el.getBoundingClientRect();

        return (
            rect.top >= 0 &&
                rect.left >= 0 &&
                rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) && /*or $(window).height() */
                rect.right <= (window.innerWidth || document.documentElement.clientWidth) /*or $(window).width() */
        );
    };

    getBodyFromHtmlDocument = function (html) {
        return html.replace(/^[\s\S]*<body.*?>|<\/body>[\s\S]*$/ig, '').trimStart().trimEnd();
    }

    $$ = function (id, context) {
        /// <summary>
        /// Searches for an ID based on ASP.NET naming container syntax.
        /// First search by ID as is, then uses attribute based lookup.
        /// Works only on single elements - not list items in enumerated
        /// containers.
        /// </summary>    
        /// <param name="id" type="var">Element ID to look up</param>    
        /// <param name="context" type="var">
        /// </param>    
        /// <returns type="" /> 
        var el = $("#" + id, context);
        if (el.length < 1)
            el = $("[id$=_" + id + "],[id*=" + id + "_]", context);
        return el;
    };

    String.prototype.htmlEncode = function () {
        var div = document.createElement('div');
        if (typeof (div.textContent) == 'string')
            div.textContent = this.toString();
        else
            div.innerText = this.toString();
        return div.innerHTML;
    };
    String.prototype.trimEnd = function (c) {
        if (c)
            return this.replace(new RegExp(c.escapeRegExp() + "*$"), '');
        return this.replace(/\s+$/, '');
    };
    String.prototype.trimStart = function (c) {
        if (c)
            return this.replace(new RegExp("^" + c.escapeRegExp() + "*"), '');
        return this.replace(/^\s+/, '');
    };
    String.prototype.repeat = function (chr, count) {
        var str = "";
        for (var x = 0; x < count; x++) {
            str += chr;
        };
        return str;
    };
    String.prototype.padL = function (width, pad) {
        if (!width || width < 1)
            return this;

        if (!pad) pad = " ";
        var length = width - this.length;
        if (length < 1) return this.substr(0, width);

        return (this.repeat(pad, length) + this).substr(0, width);
    };
    String.prototype.padR = function (width, pad) {
        if (!width || width < 1) return this;

        if (!pad) pad = " ";
        var length = width - this.length;
        if (length < 1) this.substr(0, width);

        return (this + pad.repeat(length)).substr(0, width);
    }
    String.prototype.startsWith = function (sub,nocase) {
        if (!this || this.length === 0) return false;

        if (sub && nocase)
            return sub.toLowerCase() === this.toLowerCase().substr(0, sub.length);

        return sub === this.substr(0, sub.length);
    }
    String.prototype.extract = function(startDelim, endDelim, allowMissingEndDelim, returnDelims) {
        var str = this;
        if (str.length === 0)
            return "";

        var src = str.toLowerCase();
        startDelim = startDelim.toLocaleLowerCase();
        endDelim = endDelim.toLocaleLowerCase();

        var i1 = src.indexOf(startDelim);
        if (i1 == -1)
            return "";

        var i2 = src.indexOf(endDelim, i1 + startDelim.length);

        if (!allowMissingEndDelim && i2 == -1)
            return "";

        if (allowMissingEndDelim && i2 == -1) {
            if (returnDelims)
                return str.substr(i1);

            return str.substr(i1 + startDelim.length);
        }

        if (returnDelims)
            return str.substr(i1, i2 - i1 + startDelim.length);

        return str.substr(i1 + startDelim.length, i2 - i1 - startDelim.length);
    };
    String.prototype.escapeRegExp = function () {
        return this.replace(/[.*+?^${}()|[\]\/\\]/g, "\\$0");
    };
    String.format = function (frmt, args) {
        for (var x = 0; x < arguments.length; x++) {
            frmt = frmt.replace(new RegExp("\\{" + x.toString() + "\\}", "g"), arguments[x + 1]);
        }
        return frmt;
    };
    String.prototype.format = function () {
        var a = [this];
        $.merge(a, arguments);
        return String.format.apply(this, a);
    };
    String.prototype.isNumber = function () {

        if (this.length == 0) return false;
        if ("0123456789".indexOf(this.charAt(0)) > -1)
            return true;
        return false;
    };
    var _monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    Date.prototype.formatDate = function (format) {
        var date = this;
        if (!format)
            format = "MM/dd/yyyy";

        var month = date.getMonth();
        var year = date.getFullYear();

        if (format.indexOf("yyyy") > -1)
            format = format.replace("yyyy", year.toString());
        else if (format.indexOf("yy") > -1)
            format = format.replace("yy", year.toString().substr(2, 2));

        format = format.replace("dd", date.getDate().toString().padL(2, "0"));

        var hours = date.getHours();
        if (format.indexOf("t") > -1) {
            if (hours > 11)
                format = format.replace("t", "pm");
            else
                format = format.replace("t", "am");
        }
        if (format.indexOf("HH") > -1)
            format = format.replace("HH", hours.toString().padL(2, "0"));
        if (format.indexOf("hh") > -1) {
            if (hours > 12) hours -= 12;
            if (hours == 0) hours = 12;
            format = format.replace("hh", hours.toString().padL(2, "0"));
        }
        if (format.indexOf("mm") > -1)
            format = format.replace("mm", date.getMinutes().toString().padL(2, "0"));
        if (format.indexOf("ss") > -1)
            format = format.replace("ss", date.getSeconds().toString().padL(2, "0"));

        if (format.indexOf("MMMM") > -1)
            format = format.replace("MMMM", _monthNames[month]);
        else if (format.indexOf("MMM") > -1)
            format = format.replace("MMM", _monthNames[month].substr(0, 3));
        else
            format = format.replace("MM", (month + 1).toString().padL(2, "0"));

        return format;
    };
    Number.prototype.formatNumber = function (format, option) {
        var num = this;
        var fmt = Number.getNumberFormat();
        if (format == "c") {
            num = Math.round(num * 100) / 100;
            option = option || "$";
            num = num.toLocaleString();
            var s = num.split(".");
            var p = s.length > 1 ? s[1] : '';
            return option + s[0] + fmt.d + p.padR(2, '0');
        }
        if (format.charAt(0) == "n") {
            if (format.length == 1)
                return num.toLocaleString()
            var dec = format.substr(1);
            dec = parseInt(dec);
            if (typeof (dec) != "number")
                return num.toLocaleString();
            num = num.toFixed(dec);
            var x = num.split(fmt.d);
            var x1 = x[0];
            var x2 = x.length > 1 ? fmt.d + x[1] : '';
            var rgx = /(\d+)(\d{3})/;
            while (rgx.test(x1))
                x1 = x1.replace(rgx, '$1' + fmt.c + '$2');
            return x1 + x2
        }
        if (format.charAt(0) == "f") {
            if (format.length == 1)
                return num.toString();
            var dc = format.substr(1);
            dc = parseFloat(dec);
            if (typeof (dec) != "number")
                return num.toString();
            return num.toFixed(dec);
        }
        return num.toString();
    };
    Number.getNumberFormat = function (cur) {
        var t = 1000.1.toLocaleString();
        var r = {};
        r.d = t.charAt(5);
        if (r.d.isNumber())
            r.d = t.charAt(4);
        r.c = t.charAt(1);
        if (r.c.isNumber())
            r.c = ",";
        r.s = cur || "$";
        return r;
    };
    registerNamespace = function (ns) {
        var pts = ns.split('.');
        var stk = window;
        var nsp = "";
        for (var i = 0; i < pts.length; i++) {
            var pt = pts[i];
            if (stk[pt])
                stk = stk[pt];
            else
                stk = stk[pt] = {};
        }
    };
    getUrlEncodedKey = function (key, query) {
        if (!query) query = window.location.search;
        var re = new RegExp("[?|&]" + key + "=(.*?)&");
        var matches = re.exec(query + "&");
        if (!matches || matches.length < 2)
            return "";
        return decodeURIComponent(matches[1].replace("+", " "));
    };
    setUrlEncodedKey = function (key, value, query) {

        query = query || window.location.search;
        var q = query + "&";
        var re = new RegExp("[?|&]" + key + "=.*?&");
        if (!re.test(q))
            q += key + "=" + encodeURI(value);
        else
            q = q.replace(re, "&" + key + "=" + encodeURIComponent(value) + "&");
        q = q.trimStart("&").trimEnd("&");
        return q.charAt(0) == "?" ? q : q = "?" + q;
    };
    $.fn.serializeNoViewState = function () {
        return this.find("input,textarea,select,hidden").not("#__VIEWSTATE,#__EVENTVALIDATION").serialize();
    };

    if (!this.assert) {
        this.assert = function (cond, msg) {
            if (cond) return;
            if (!msg) msg = "";
            alert("Assert failed\r\n" + (msg ? msg : "") + "\r\n" +
              (arguments.callee.caller ? "in " + arguments.callee.caller.toString() : ""));
        }
    }

    $.expr[":"].containsNoCase = function (el, i, m) {
        var search = m[3];
        if (!search) return false;
        return new RegExp(search, "i").test($(el).text());
    };

    $.fn.searchFilter = function (options) {
        var opt = $.extend({
            // target selector
            targetSelector: "",
            // number of characters before search is applied
            charCount: 1,
            onSelected: null
        }, options);

        return this.each(function () {
            var $el = $(this);

            function keyup() {
                var search = $(this).val();

                var $target = $(opt.targetSelector);
                $target.show();

                if (opt.onSelected)
                    opt.onSelected($target);

                if (search && search.length >= opt.charCount)
                    $target.not(":containsNoCase(" + search + ")").hide();
            }

            $el.keyup(keyup);
        });
    };

    /*
    http://www.JSON.org/json2.js
    2009-04-16
    Public Domain.
    */
    if (!this.JSON) { this.JSON = {}; }
    (function () {
        function f(n) { return n < 10 ? '0' + n : n; }
        if (typeof Date.prototype.toJSON !== 'function') {
            Date.prototype.toJSON = function (key) {
                return isFinite(this.valueOf()) ? this.getUTCFullYear() + '-' +
f(this.getUTCMonth() + 1) + '-' +
f(this.getUTCDate()) + 'T' +
f(this.getUTCHours()) + ':' +
f(this.getUTCMinutes()) + ':' +
f(this.getUTCSeconds()) + 'Z' : null;
            }; String.prototype.toJSON = Number.prototype.toJSON = Boolean.prototype.toJSON = function (key) { return this.valueOf(); };
        }
        var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, gap, indent, meta = { '\b': '\\b', '\t': '\\t', '\n': '\\n', '\f': '\\f', '\r': '\\r', '"': '\\"', '\\': '\\\\' }, rep; function quote(string) { escapable.lastIndex = 0; return escapable.test(string) ? '"' + string.replace(escapable, function (a) { var c = meta[a]; return typeof c === 'string' ? c : '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4); }) + '"' : '"' + string + '"'; }
        function str(key, holder) {
            var i, k, v, length, mind = gap, partial, value = holder[key]; if (value && typeof value === 'object' && typeof value.toJSON === 'function') { value = value.toJSON(key); }
            if (typeof rep === 'function') { value = rep.call(holder, key, value); }
            switch (typeof value) {
                case 'string': return quote(value); case 'number': return isFinite(value) ? String(value) : 'null'; case 'boolean': case 'null': return String(value); case 'object': if (!value) { return 'null'; }
                    gap += indent; partial = []; if (Object.prototype.toString.apply(value) === '[object Array]') {
                        length = value.length; for (i = 0; i < length; i += 1) { partial[i] = str(i, value) || 'null'; }
                        v = partial.length === 0 ? '[]' : gap ? '[\n' + gap +
partial.join(',\n' + gap) + '\n' +
mind + ']' : '[' + partial.join(',') + ']'; gap = mind; return v;
                    }
                    if (rep && typeof rep === 'object') { length = rep.length; for (i = 0; i < length; i += 1) { k = rep[i]; if (typeof k === 'string') { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ': ' : ':') + v); } } } } else { for (k in value) { if (Object.hasOwnProperty.call(value, k)) { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ': ' : ':') + v); } } } }
                    v = partial.length === 0 ? '{}' : gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' +
mind + '}' : '{' + partial.join(',') + '}'; gap = mind; return v;
            }
        }
        if (typeof JSON.stringify !== 'function') {
            JSON.stringify = function (value, replacer, space) {
                var i; gap = ''; indent = ''; if (typeof space === 'number') { for (i = 0; i < space; i += 1) { indent += ' '; } } else if (typeof space === 'string') { indent = space; }
                rep = replacer; if (replacer && typeof replacer !== 'function' && (typeof replacer !== 'object' || typeof replacer.length !== 'number')) { throw new Error('JSON.stringify'); }
                return str('', { '': value });
            };
        }
        if (typeof JSON.parse !== 'function') {
            JSON.parse = function (text, reviver) {
                var j; function walk(holder, key) {
                    var k, v, value = holder[key]; if (value && typeof value === 'object') { for (k in value) { if (Object.hasOwnProperty.call(value, k)) { v = walk(value, k); if (v !== undefined) { value[k] = v; } else { delete value[k]; } } } }
                    return reviver.call(holder, key, value);
                }
                cx.lastIndex = 0; if (cx.test(text)) {
                    text = text.replace(cx, function (a) {
                        return '\\u' +
('0000' + a.charCodeAt(0).toString(16)).slice(-4);
                    });
                }
                if (/^[\],:{}\s]*$/.test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, '@').replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, ']').replace(/(?:^|:|,)(?:\s*\[)+/g, ''))) { j = eval('(' + text + ')'); return typeof reviver === 'function' ? walk({ '': j }, '') : j; }
                throw new SyntaxError('JSON.parse');
            };
        }
    }());


    if (this.JSON && !this.JSON.dateParser) {
        var reISO = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.{0,1}\d*))(?:Z|(\+|-)([\d|:]*))?$/;
        var reMsAjax = /^\/Date\((d|-|.*)\)[\/|\\]$/;

        /// <summary>
        /// set this if you want MS Ajax Dates parsed
        /// before calling any of the other functions
        /// </summary>
        JSON.parseMsAjaxDate = false;

        JSON.useDateParser = function (reset) {
            /// <summary>
            /// Globally enables JSON date parsing for JSON.parse().
            /// replaces the 
            /// </summary>    
            /// <param name="reset" type="bool">when set restores the original JSON.parse() function</param>

            // if any parameter is passed reset
            if (typeof reset != "undefined") {
                if (JSON._parseSaved) {
                    JSON.parse = JSON._parseSaved;
                    JSON._parseSaved = null;
                }
            } else {
                if (!JSON.parseSaved) {
                    JSON._parseSaved = JSON.parse;
                    JSON.parse = JSON.parseWithDate;
                }
            }
        };

        JSON.dateParser = function (key, value) {
            /// <summary>
            /// Globally enables JSON date parsing for JSON.parse().
            /// Replaces the default JSON.parse() method and adds
            /// the datePaser() extension to the processing chain.
            /// </summary>    
            /// <param name="key" type="string">property name that is parsed</param>
            /// <param name="value" type="any">property value</param>
            /// <returns type="date">returns date or the original value if not a date string</returns>
            if (typeof value === 'string') {
                var a = reISO.exec(value);
                if (a)
                    return new Date(value);

                if (!JSON.parseMsAjaxDate)
                    return value;

                a = reMsAjax.exec(value);
                if (a) {
                    var b = a[1].split(/[-+,.]/);
                    return new Date(b[0] ? +b[0] : 0 - +b[1]);
                }
            }
            return value;
        };

        JSON.parseWithDate = function (json) {
            /// <summary>
            /// Wrapper around the JSON.parse() function that adds a date
            /// filtering extension. Returns all dates as real JavaScript dates.
            /// </summary>    
            /// <param name="json" type="string">JSON to be parsed</param>
            /// <returns type="any">parsed value or object</returns>
            var parse = JSON._parseSaved ? JSON._parseSaved : JSON.parse;
            try {
                var res = parse(json, JSON.dateParser);
                return res;
            } catch (e) {
                // orignal error thrown has no error message so rethrow with message
                throw new Error("JSON content could not be parsed");
            }
        };

        JSON.dateStringToDate = function (dtString, nullDateVal) {
            /// <summary>
            /// Converts a JSON ISO or MSAJAX date or real date a date value.
            /// Supports both JSON encoded dates or plain date formatted strings
            /// (without the JSON string quotes).
            /// If you pass a date the date is returned as is. If you pass null
            /// null or the nullDateVal is returned.
            /// </summary>    
            /// <param name="dtString" type="var">Date String in ISO or MSAJAX format</param>
            /// <param name="nullDateVal" type="var">value to return if date can't be parsed</param>
            /// <returns type="date">date or the nullDateVal (null by default)</returns> 
            if (!nullDateVal)
                nullDateVal = null;

            if (!dtString)
                return nullDateVal; // empty

            if (dtString.getTime)
                return dtString; // already a date

            if (dtString[0] === '"' || dtString[0] === "'")
                // strip off JSON quotes
                dtString = dtString.substr(1, dtString.length - 2);

            var a = reISO.exec(dtString);
            if (a)
                return new Date(dtString);

            if (!JSON.parseMsAjaxDate)
                return nullDateVal;

            a = reMsAjax.exec(dtString);
            if (a) {
                var b = a[1].split(/[-,.]/);
                return new Date(+b[0]);
            }
            return nullDateVal;
        };
    }
})(jQuery);