using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using MvcWords.Domain;
using  LjcWebApp.Services.Word;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class WordController : Controller
    {
        //
        // GET: /Word/
        WordService _service = new WordService();

        public ActionResult Index(string likeStr = null)
        {
            var list = new List<word_tb>();
            if (!string.IsNullOrWhiteSpace(likeStr))
            {
                list = _service.SearchWords(likeStr);
            }
            return View(list);
        }

        //
        // GET: /Word/Details/5

        public ActionResult Details(int id)
        {
            var word = _service.GetWord(id);
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
            var word = _service.GetWord(id);
            if (word != null)
            {
                return View(word);
            }
            return View();
        }

        //
        // POST: /Word/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var word = new word_tb();

                    if (TryUpdateModel(word, collection) && _service.UpdateWord(word))
                    {
                        return RedirectToAction("Index", new { likeStr = word.Spelling });
                    }
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Word/Delete/5

        public ActionResult Delete(int id)
        {
            var word = _service.GetWord(id);
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

                if (_service.DeleteWord(id))
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
