﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using LjcWebApp.Models.entity;
using LjcWebApp.Models.ViewModels;
using LjcWebApp.Services.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LjcWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IOptions<AppSettings> _appSettings;
        private MyUserService _service = new MyUserService();

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
                if (_service.IsPasswordCorrect(model.UserName.Trim(), model.Password.Trim()))
                {
                    var userNameStr = model.UserName;
                    var passwordStr = model.Password;
                    var claims = new List<Claim>()
                    {
                        new Claim( ClaimTypes.Name, userNameStr),
                        new Claim( ClaimTypes.UserData, userNameStr + passwordStr)
                    };
                    var identity = new ClaimsIdentity(claims, "MyClaimsLogin");
                    var principal = new ClaimsPrincipal(identity);
                    HttpContext.Authentication.SignInAsync("MyCookieMiddlewareInstance", principal);
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

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(AccountModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var userNameStr = model.UserName;
                var passwordStr = model.Password;
                if (!_service.IsUserExist(userNameStr))
                {
                    _service.Add(new MyUser() { UserName = userNameStr, Password = passwordStr });
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("UserName", "用户名已存在");
                return View(model);
            }

            return View(model);
        }

    }
}
