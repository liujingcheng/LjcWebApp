using System;
using System.Collections.Generic;
using LjcWebApp.Helper;
using LjcWebApp.Services.Account;
using LjcWebApp.Services.DataLoad;
using Microsoft.AspNetCore.Mvc;
using LjcWebApp.Services.Word;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class WordController : Controller
    {
        //
        // GET: /Word/
        WordService _service;
        public WordService WordService
        {
            get
            {
                if (HttpContext.User.Identity.Name != "ljcwyc")
                {
                    throw new Exception();
                }
                if (_service == null)
                {
                    return new WordService()
                    {
                        CurrentUser = new MyUserService().GetByUserName(HttpContext.User.Identity.Name)
                    };
                }
                return _service;
            }
        }

        public ActionResult Index(string likeStr = null)
        {
            var list = new List<word_tb>();
            if (!string.IsNullOrWhiteSpace(likeStr))
            {
                list = WordService.SearchWords(likeStr);
            }
            return View(list);
        }

        //
        // GET: /Word/Details/5

        public ActionResult Details(int id)
        {
            var word = WordService.GetWord(id);
            if (word != null)
            {
                return View(word);
            }
            return View();
        }

        //
        // GET: /Word/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Word/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Word/Edit/5

        public ActionResult Edit(int id)
        {
            var word = WordService.GetWord(id);
            if (word != null)
            {
                return View(word);
            }
            return View();
        }

        //
        // POST: /Word/Edit/5

        [HttpPost]
        public ActionResult Edit(word_tb word)
        {
            if (ModelState.IsValid)
            {
                if (WordService.UpdateWord(word))
                {
                    return RedirectToAction("Index", "Word");
                }
            }
            return View();
        }

        //
        // GET: /Word/Delete/5

        public ActionResult Delete(int id)
        {
            var word = WordService.GetWord(id);
            if (word != null)
            {
                return View(word);
            }
            return View();
        }

        //
        // POST: /Word/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                var word = new word_tb();

                if (WordService.DeleteWord(id))
                {
                    return RedirectToAction("Index", new { likeStr = word.Spelling });
                }
                return View();
            }
            catch
            {
                return View();
            }
        }
    }
}
