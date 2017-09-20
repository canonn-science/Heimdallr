if (!window.canonnEmbed)
{

    window.canonnEmbed = function () {
        this.maxWidth = '1600px';
        this.maxHeight = '900px';
        this.queryURL = 'http://localhost:56711/Lookup/';
        this.frameCount = 0;

        this.handleMessage = function messageHandler(m) {
            var data = '';
            try {
                data = JSON.parse(m.data);
            } catch (e) {
                data = { message: e.data }
            }

            if ('canonn_content_update' === data.message) {
                //Get the query item from the URL
                var embedItem = data.href.split('/').slice(-1).pop();
                var withClass = embedItem.replace(/[\W_]+/g, "");

                //Find any frame(s)
                var matchFrames = document.querySelectorAll('.canonn-embed-iframe-' + withClass);

                for (i = 0; i < matchFrames.length; ++i) {
                    //Get blockquote info
                    var frame = matchFrames[i];

                    frame.style.height = data.height
                    
                }


            }

        };

        this.init = function canonnInit() {

            var bquotes = document.querySelectorAll('blockquote.canonn-embed'), i;

            for (i = 0; i < bquotes.length; ++i) {
                //Get blockquote info
                var bQ = bquotes[i];
                var embedCode = '';
                if (bQ.getAttribute) {
                    embedCode = bQ.getAttribute("data-value");
                }

                if (!embedCode) {
                    return;
                }

                //New frame
                window.canonnEmbed.frameCount++;

                var newFrame = document.createElement("iframe");
                newFrame.id = "canonn-embed-iframe-" + window.canonnEmbed.frameCount;
                newFrame.className = "canonn-embed-iframe-" + embedCode.replace(/[\W_]+/g, "");

                newFrame.style.maxWidth = window.canonnEmbed.maxWidth;
                newFrame.style.maxHeight = window.canonnEmbed.maxHeight;
                newFrame.style.width = '100%';

                newFrame.style.background = 'transparent';
                newFrame.style.border = 'none';
                newFrame.style.margin = "10px 0px";
                newFrame.style.padding = 0;
                newFrame.scrolling = "no";

                newFrame.src = window.canonnEmbed.queryURL + encodeURIComponent(embedCode);

                //Replace with new element
                bQ.parentNode.replaceChild(newFrame, bQ);
            }
        }

        if (window.addEventListener) {
            window.addEventListener('message', this.handleMessage);
        } else {
            window.attachEvent('onmessage', this.handleMessage);
        }

        var winEvent = window.addEventListener ? window.addEventListener : window.attachEvent;

        return this;
    }();

   
}

window.canonnEmbed.init();

/* window.edsmEmbed.createIframe ? edsmEmbed.createIframe() : edsmEmbed.tasks++; */



///**
// * Elite: Dangerous Star Map
// *
// * @link        https://www.edsm.net/
// * @copyright   Copyright (c) 2015-2017 EDSM.
// */

//    function () {
//        var maxWidth = 840;

//        // Handle received message from iframe
//        var o = window.addEventListener ? "addEventListener" : "attachEvent",
//            p = window[o],
//            q = "attachEvent" == o ? "onmessage" : "message";

//        p(q, function (a) {
//            var jsonMessage;

//            try { jsonMessage = JSON.parse(a.data); }
//            catch (exception) { jsonMessage = { message: a.data }; }

//            // Update iFrame size when requested
//            if ("resize_edsm" === jsonMessage.message) {
//                var d = /tools\/embed\/index\/query\/(.+)/g,
//                    e = d.exec(jsonMessage.href)[1];
//                UpdateSpecificIframeHeight(jsonMessage.height, replaceSeparators(e));
//            }
//        }, !1);

//        var createInlineCSS = function (css) {
//            var head = document.getElementsByTagName("head")[0],
//                style = document.createElement("style");

//            style.type = "text/css",
//                style.styleSheet ? style.styleSheet.cssText = css : style.appendChild(document.createTextNode(css)),
//                head.appendChild(style);
//        },

//            UpdateSpecificIframeHeight = function (height, b) {
//                var d = "#edsm-embed-iframe-" + b + " { height: " + height + "px !important;}";
//                createInlineCSS(d)
//            },

//            createDefaultCSS = function () {
//                var css = ".edsm-embed-iframe { background: transparent;border: none; }";
//                createInlineCSS(css);
//            },

//            getIframeSrc = function (query, language) {
//                return ['https://www.edsm.net', language, 'tools', 'embed', 'index', 'query', query].join("/");
//            },

//            replaceSeparators = function (query) {
//                query = decodeURIComponent(query);

//                return query.replace(/\//g, "-")
//                    .replace(/::/g, "-")
//                    .replace(/#/g, "-")
//                    .replace(/\s/g, "-");
//            },

//            createInlineIframe = function (blockquote) {
//                if (blockquote) {
//                    var query,
//                        language;

//                    try { query = blockquote.getAttribute("data-value"); }
//                    catch (exception) { return void console.error(exception); }

//                    try { language = blockquote.getAttribute("lang"); }
//                    catch (exception) { language = 'en'; }

//                    // Create iframe                
//                    iframe = document.createElement("iframe");
//                    iframe.style.maxWidth = maxWidth + "px",
//                        iframe.style.width = "100%",
//                        iframe.className = "edsm-embed-iframe",
//                        iframe.style.margin = "10px 0px",
//                        iframe.style.padding = 0,
//                        iframe.scrolling = "no",

//                        iframe.src = getIframeSrc(query, language),
//                        iframe.id = "edsm-embed-iframe-" + replaceSeparators(query);

//                    // Replace current blockquote with iframe
//                    blockquote.parentNode.replaceChild(iframe, blockquote);
//                }
//            };


//        // INIT IFRAMES
//        createDefaultCSS();
//        window.edsmEmbed.createIframe = function () {
//            createInlineIframe(document.querySelector("blockquote.edsm-embed"))
//        };
//        for (var w = 0; w < window.edsmEmbed.tasks; w++) {
//            createInlineIframe(document.querySelector("blockquote.edsm-embed"))
//        }
//    }();