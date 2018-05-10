// Performs tests to see if a stylesheet was loaded successfully, if the stylesheet failed to load, appends a new
// link tag pointing to the local copy of the stylesheet before performing the next check.
(function (window, document) {
    "use strict";

    var fallbacks = [
        {
            // metaName - The name of the meta tag that the test is performed on. The meta tag must have a class from the
            //            relevant stylesheet on it so it is styled and a test can be performed against it. E.g. for
            //            font awesome the <meta name="x-font-awesome-stylesheet-fallback-test" class="fa"> meta tag is
            //            added. The 'fa' class causes the font awesome style to be applied to it.
            metaName: "x-font-awesome-stylesheet-fallback-test",
            // test - The test to perform against the meta tag. Checks to see if the Font awesome styles loaded
            //        successfully by checking that the font-family of the meta tag is 'FontAwesome'.
            test: function (meta) {
                return window.getComputedStyle(meta, null).getPropertyValue('font-family') === "FontAwesome";
            },
            // href - The URL to the fallback stylesheet.
            href: "/css/font-awesome.css"
        }
    ];

    var metas = document.getElementsByTagName("meta");

    for (var i = 0; i < fallbacks.length; ++i) {
        var fallback = fallbacks[i];

        for (var j = 0; j < metas.length; ++j) {
            var meta = metas[j];
            if (meta.getAttribute("name") === fallback.metaName) {
                if (!fallback.test(meta)) {
                    var link = document.createElement("link");
                    link.href = fallback.href;
                    link.rel = "stylesheet";
                    document.getElementsByTagName("head")[0].appendChild(link);
                }
                break;
            }
        }
    }
})(window, document);
// Performs tests to see if a script was loaded successfully, if the script failed to load, appends a new script tag
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

//# sourceMappingURL=site.js.map