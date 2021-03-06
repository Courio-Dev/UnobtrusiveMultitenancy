﻿namespace PuzzleCMS.WebHost.Controllers
{
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using PuzzleCMS.WebHost.Constants;
    using PuzzleCMS.WebHost.Settings;

    /// <summary>
    /// HomeController.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IOptions<AppSettings> appSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// Contructor of HomeController.
        /// </summary>
        public HomeController(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings;
        }

        /// <summary>
        /// Index.
        /// </summary>
        [Route("")]
        [Route("Home")]
        [Route("Home/Index")]
        [HttpGet("", Name = HomeControllerRoute.GetIndex)]
        public IActionResult Index() => View(HomeControllerAction.Index);

        /// <summary>
        /// About.
        /// </summary>
        [HttpGet("about", Name = HomeControllerRoute.GetAbout)]
        public IActionResult About() => View(HomeControllerAction.About);

        /// <summary>
        /// Contact.
        /// </summary>
        [HttpGet("contact", Name = HomeControllerRoute.GetContact)]
        public IActionResult Contact() => View(HomeControllerAction.Contact);

        /// <summary>
        /// Tells search engines (or robots) how to index your site.
        /// The reason for dynamically generating this code is to enable generation of the full absolute sitemap URL
        /// and also to give you added flexibility in case you want to disallow search engines from certain paths. The
        /// sitemap is cached for one day, adjust this time to whatever you require. See
        /// http://rehansaeed.com/dynamically-generating-robots-txt-using-asp-net-mvc/.
        /// </summary>
        /// <returns>The robots text for the current site.</returns>
        [ResponseCache(CacheProfileName = CacheProfileName.RobotsText)]
        [Route("robots.txt", Name = HomeControllerRoute.GetRobotsText)]
        public IActionResult RobotsText() => Content(string.Empty, "text/plain", Encoding.UTF8);

        /// <summary>
        /// Gets the Open Search XML for the current site. You can customize the contents of this XML here. The open
        /// search action is cached for one day, adjust this time to whatever you require. See
        /// http://www.hanselman.com/blog/CommentView.aspx?guid=50cc95b1-c043-451f-9bc2-696dc564766d
        /// http://www.opensearch.org.
        /// </summary>
        /// <returns>The Open Search XML for the current site.</returns>
        [ResponseCache(CacheProfileName = CacheProfileName.OpenSearchXml)]
        [Route("opensearch.xml", Name = HomeControllerRoute.GetOpenSearchXml)]
        public IActionResult OpenSearchXml() => Content(string.Empty, "application/xml", Encoding.UTF8);

        /// <summary>
        /// Gets the sitemap XML for the current site. You can customize the contents of this XML from the.
        /// http://www.sitemaps.org/protocol.html.
        /// </summary>
        /// <param name="index">The index of the sitemap to retrieve. <c>null</c> if you want to retrieve the root
        /// sitemap file, which may be a sitemap index file.</param>
        /// <returns>The sitemap XML for the current site.</returns>
        [Route("sitemap.xml", Name = HomeControllerRoute.GetSitemapXml)]
        public async Task<IActionResult> SitemapXml(int? index = null)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            string content = "application / xml";

            if (content == null)
            {
                return BadRequest("Sitemap index is out of range.");
            }

            return Content(content, "application/xml", Encoding.UTF8);
        }

        /// <summary>
        /// Gets the manifest JSON for the current site. This allows you to customize the icon and other browser
        /// settings for Chrome/Android and FireFox (FireFox support is coming). See https://w3c.github.io/manifest/
        /// for the official W3C specification. See http://html5doctor.com/web-manifest-specification/ for more
        /// information. See https://developer.chrome.com/multidevice/android/installtohomescreen for Chrome's
        /// implementation.
        /// </summary>
        /// <returns>The manifest JSON for the current site.</returns>
        [ResponseCache(CacheProfileName = CacheProfileName.ManifestJson)]
        [Route("manifest.json", Name = HomeControllerRoute.GetManifestJson)]
        public ContentResult ManifestJson() => Content(string.Empty, "application/json", Encoding.UTF8);
    }
}
