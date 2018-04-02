namespace PuzzleCMS.WebHost.Services
{
    using Microsoft.AspNetCore.Mvc;

    public class BrowserConfigService : IBrowserConfigService
    {
        private readonly IUrlHelper urlHelper;

        public BrowserConfigService(IUrlHelper urlHelper)
        {
            this.urlHelper = urlHelper;
        }

        string IBrowserConfigService.GetBrowserConfigXml()
        {
            throw new System.NotImplementedException();
        }
    }
}
