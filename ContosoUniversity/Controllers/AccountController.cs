using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

namespace ContosoUniversity.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(
                OpenIdConnectAuthenticationDefaults.AuthenticationType);
            return RedirectToAction("Index", "Home");
        }
    }
}
