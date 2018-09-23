﻿// Performs tests to see if a script was loaded successfully, if the script failed to load, appends a new script tag
// pointing to the local copy of the script and then waits for it to load before performing the next check.
// Example: Bootstrap is dependant on jQuery. If loading jQuery from the CDN fails, this script loads the jQuery
//          fallback and waits for it to finish loading before attempting the next fallback test.
(function (document) {
    "use strict";

    var fallbacks = [
        // test - Tests whether the script loaded successfully or not. Returns true if the script loaded successfully or
        //        false if the script failed to load and the fallback is required.
        // src - The URL to the fallback script.
        { test: function () { return window.jQuery; }, src: "/js/jquery.js" },
        { test: function () { return window.jQuery.validator; }, src: "/js/jquery-validate.js" },
        { test: function () { return window.jQuery.validator.unobtrusive; }, src: "/js/jquery-validate-unobtrusive.js" },
        { test: function () { return window.jQuery.fn.modal; }, src: "/js/bootstrap.js" }
    ];

    var check = function (fallbacks, i) {
        if (i < fallbacks.length) {
            var fallback = fallbacks[i];
            if (fallback.test()) {
                check(fallbacks, i + 1);
            }
            else {
                var script = document.createElement("script");
                script.onload = function () {
                    check(fallbacks, i + 1);
                };
                script.src = fallback.src;
                document.getElementsByTagName("body")[0].appendChild(script);
            }
        }
    };
    check(fallbacks, 0);
})(document);