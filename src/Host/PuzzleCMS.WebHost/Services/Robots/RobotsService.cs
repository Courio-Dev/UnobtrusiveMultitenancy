namespace PuzzleCMS.WebHost.Services
{
    using Microsoft.AspNetCore.Mvc;
    using System.Text;

    public sealed class RobotsService : IRobotsService
    {
        private readonly IUrlHelper urlHelper;

        public RobotsService(IUrlHelper urlHelper)
        {
            this.urlHelper = urlHelper;
        }

        /// <summary>
        /// Gets the robots text for the current site. This tells search engines (or robots) how to index your site.
        /// The reason for dynamically generating this code is to enable generation of the full absolute sitemap URL
        /// and also to give you added flexibility in case you want to disallow search engines from certain paths. See
        /// http://rehansaeed.com/dynamically-generating-robots-txt-using-asp-net-mvc/
        /// Note: Disallowing crawling of JavaScript or CSS files in your sites robots.txt directly harms how well
        /// Google's algorithms render and index your content and can result in suboptimal rankings.
        /// </summary>
        /// <returns>The robots text for the current site.</returns>
        public string GetRobotsText()
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Allow all robots.
            stringBuilder.AppendLine("user-agent: *");

            // Tell all robots not to index any directories.
            // stringBuilder.AppendLine("disallow: /");

            // Tell all robots not to index everything under the following directory.
            // stringBuilder.AppendLine("disallow: /SomeRelativePath");

            // Tell all robots to to index any of the error pages.
            stringBuilder.AppendLine("disallow: /error/");

            return stringBuilder.ToString();
        }
    }
}