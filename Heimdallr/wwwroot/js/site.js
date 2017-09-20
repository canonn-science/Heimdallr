/*
 * Derived from EDSM iframe content script 
 *
 * Elite: Dangerous Star Map
 *
 * @link        https://www.edsm.net/
 * @copyright   Copyright (c) 2015-2017 EDSM.
 * used with permission
*/

//Used to send a message to the parent page so it can update the content size
var sendResize = function(){
    message = JSON.stringify({
        message: 'canonn_content_update',
        href: window.location.href,
        height: Math.max(
            document.body.scrollHeight,
            document.body.offsetHeight,
            document.documentElement.clientHeight,
            document.documentElement.scrollHeight,
            document.documentElement.offsetHeight
        )
    });
    parent.postMessage(message,'*');
};

window.onload   = sendResize;
window.onresize = sendResize;
    