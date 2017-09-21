if (!window.canonnEmbed)
{

    window.canonnEmbed = function () {
        this.maxWidth = '840px';
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