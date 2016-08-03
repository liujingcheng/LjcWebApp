using LjcWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LjcWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;

        public AccountController(IOptions<AppSettings> settings)
        {
            _appSettings = settings;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AccountModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var userNameStr = _appSettings.Value.UserName;
                var passwordStr = _appSettings.Value.Password;
                if (model.UserName == userNameStr && model.Password == passwordStr)
                {
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError("Password", "用户名或密码错误");
                return View(model);
            }

            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

    }
}
