using System;
using System.Net;
using System.Web;
using System.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Web.Routing;
using System.Web.Security;
using MvcWords.Web.Common;
using MvcWords.Web.Models;
using LjcWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace LjcWebApp.Controllers
{
    public class AccountController : Controller
    {
        #region 登陆处理

        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Login(string returnUrl)
        {
            Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
            Response.AddHeader("X-Unauthorized", "true");//用于前台Ajax请求时,如果cookies过期要跳回登录页

            if (ControllerContext.HttpContext.Request.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            else
            {
                Session.Abandon();
            }

            return View(new AccountModels());
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(AccountModels userModel, string returnUrl)
        {
            Response.AddHeader("X-Unauthorized", "true");

            if (ModelState.ValidateFields(new string[] { "UesrName", "Password" }))
            {
                var userNameStr = WebConfigurationManager.AppSettings["UserName"];
                var passwordStr = WebConfigurationManager.AppSettings["Password"];

                if (userModel.UserName != userNameStr || userModel.Password != passwordStr)
                {
                    ModelState.AddModelError("Password", "用户名或密码错误！");
                    return View("~/Views/Account/Login.cshtml");
                }

                //授权
                FormsAuthentication.SetAuthCookie(userModel.UserName, false);

                return RedirectToLocal(returnUrl);
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            ModelState.AddModelError("Password", "用户名或密码错误！");
            return View();
        }

        public ActionResult RedirectToLocal(string returnUrl)
        {
            if (!Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Control", "TimeStatistic");
            }

            return Redirect(returnUrl);
        }

        #endregion

        #region 退出登录

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Login", "Account");
        }

        #endregion

    }
}
