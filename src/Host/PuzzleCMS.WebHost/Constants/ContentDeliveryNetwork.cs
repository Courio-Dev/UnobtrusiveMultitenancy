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

            /// <summary>
            /// JQueryUrl.
            /// </summary>
            public const string JQueryUrl = "https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js";
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

            /// <summary>
            /// FontAwesomeUrl.
            /// </summary>
            public const string FontAwesomeUrl = "https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css";
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

            /// <summary>
            /// JQueryValidateUrl.
            /// </summary>
            public const string JQueryValidateUrl = "https://ajax.aspnetcdn.com/ajax/jquery.validate/1.16.0/jquery.validate.min.js";

            /// <summary>
            /// JQueryValidateUnobtrusiveUrl.
            /// </summary>
            public const string JQueryValidateUnobtrusiveUrl = "https://ajax.aspnetcdn.com/ajax/mvc/5.2.3/jquery.validate.unobtrusive.min.js";

            /// <summary>
            /// BootstrapUrl.
            /// </summary>
            public const string BootstrapUrl = "https://ajax.aspnetcdn.com/ajax/bootstrap/3.3.7/bootstrap.min.js";
        }
    }
}
