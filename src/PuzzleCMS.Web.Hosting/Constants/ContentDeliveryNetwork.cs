namespace PuzzleCMS.WebHost.Constants
{
    /// <summary>
    /// List of CDN.
    /// </summary>
    public static class ContentDeliveryNetwork
    {
        /// <summary>
        /// Google.
        /// </summary>
        public static class Google
        {
            /// <summary>
            /// Domain.
            /// </summary>
            public const string Domain = "ajax.googleapis.com";

#pragma warning disable S1075 // URIs should not be hardcoded
            /// <summary>
            /// JQueryUrl.
            /// </summary>
            public const string JQueryUrl = "https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js";
#pragma warning restore S1075 // URIs should not be hardcoded
        }

        /// <summary>
        /// MaxCdn.
        /// </summary>
        public static class MaxCdn
        {
            /// <summary>
            /// Domain.
            /// </summary>
            public const string Domain = "maxcdn.bootstrapcdn.com";

#pragma warning disable S1075 // URIs should not be hardcoded
            /// <summary>
            /// FontAwesomeUrl.
            /// </summary>
            public const string FontAwesomeUrl = "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css";
#pragma warning restore S1075 // URIs should not be hardcoded
        }

        /// <summary>
        /// Microsoft.
        /// </summary>
        public static class Microsoft
        {
            /// <summary>
            /// Domain.
            /// </summary>
            public const string Domain = "ajax.aspnetcdn.com";

#pragma warning disable S1075 // URIs should not be hardcoded
            /// <summary>
            /// JQueryValidateUrl.
            /// </summary>
            public const string JQueryValidateUrl = "https://ajax.aspnetcdn.com/ajax/jquery.validate/1.16.0/jquery.validate.min.js";
#pragma warning restore S1075 // URIs should not be hardcoded

#pragma warning disable S1075 // URIs should not be hardcoded
            /// <summary>
            /// JQueryValidateUnobtrusiveUrl.
            /// </summary>
            public const string JQueryValidateUnobtrusiveUrl = "https://ajax.aspnetcdn.com/ajax/mvc/5.2.3/jquery.validate.unobtrusive.min.js";
#pragma warning restore S1075 // URIs should not be hardcoded

#pragma warning disable S1075 // URIs should not be hardcoded
            /// <summary>
            /// BootstrapUrl.
            /// </summary>
            public const string BootstrapUrl = "https://ajax.aspnetcdn.com/ajax/bootstrap/3.3.7/bootstrap.min.js";
#pragma warning restore S1075 // URIs should not be hardcoded
        }
    }
}
