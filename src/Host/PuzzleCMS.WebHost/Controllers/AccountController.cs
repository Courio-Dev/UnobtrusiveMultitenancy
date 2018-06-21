namespace PuzzleCMS.WebHost.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// AccountController.
    /// </summary>
    [AllowAnonymous]
    public class AccountController : Controller
    {
        /// <summary>
        /// Login.
        /// </summary>
        public IActionResult Login(string returnUrl = null)
        {
            return RedirectToLocal(returnUrl);
        }

        /// <summary>
        /// Forbidden.
        /// </summary>
        public IActionResult Forbidden() => View();

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
