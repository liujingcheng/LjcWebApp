using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using  LjcWebApp.Services.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        //
        // GET: /question/
        QuestionService _service = new QuestionService();

        public ActionResult Index()
        {
            var list = _service.GetList();
            return View(list);
        }

        public ActionResult Search(string likeStr = null)
        {
            var list = new List<question>();
            if (!string.IsNullOrWhiteSpace(likeStr))
            {
                list = _service.Search(likeStr);
            }
            return View(list);
        }

        //
        // GET: /question/Details/5

        public ActionResult Details(string id)
        {
            var entity = _service.Get(id);
            if (entity != null)
            {
                return View(entity);
            }
            return View();
        }

        //
        // GET: /question/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /question/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var entity = new question();
                    TryUpdateModelAsync(entity);
                    if (_service.Add(entity))
                    {
                        return RedirectToAction("Index", new { likeStr = entity.QuestionMember });
                    }
                }
                return View();
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public string QuickCreate(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new ArgumentNullException("filePath");
                }
                if (!_service.DeleteAll()) return "清空所有问题失败！";

                ImportFromFile(filePath);

                return "导入成功";
            }
            catch (Exception ex)
            {
                return "导入失败：" + ex.Message + "," + ex.StackTrace;
            }
        }

        //
        // GET: /question/Edit/5

        public ActionResult Edit(string id)
        {
            var entity = _service.Get(id);
            if (entity != null)
            {
                return View(entity);
            }
            return View();
        }

        //
        // POST: /question/Edit/5

        [HttpPost]
        public ActionResult Edit(string id, FormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var entity = new question();
                    TryUpdateModelAsync(entity);
                    if (_service.Update(entity))
                    {
                        return RedirectToAction("Index");
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
        // GET: /question/Delete/5

        public ActionResult Delete(string id)
        {
            var word = _service.Get(id);
            if (word != null)
            {
                return View(word);
            }
            return View();
        }

        //
        // POST: /question/Delete/5

        [HttpPost]
        public ActionResult Delete(string id, FormCollection collection)
        {
            try
            {
                var word = new word_tb();

                if (_service.Delete(id))
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

        private void ImportFromFile(string filePath)
        {
            var rFile = new FileStream(filePath, FileMode.Open);
            var sr = new StreamReader(rFile, Encoding.GetEncoding("gb2312"));//读取中文简体编码GB2312

            var listStr = new List<string>();
            while (!sr.EndOfStream)
            {
                var str = sr.ReadLine();
                if (string.IsNullOrEmpty(str) || !str.StartsWith("* ")) continue;
                listStr.Add(str.Substring(2));
            }
            sr.Dispose();

            int sort = 1;
            foreach (var lineStr in listStr)
            {
                var entity = new question();
                entity.QuestionMember = lineStr;
                entity.FullScore = 10;
                entity.Sort = sort++;
                _service.Add(entity);
            }

        }
    }
}
