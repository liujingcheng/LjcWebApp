﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LjcWebApp.Services.Account;
using Microsoft.AspNetCore.Mvc;
using LjcWebApp.Services.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LjcWebApp.Controllers
{
    [Authorize]
    public class QuestionController : Controller
    {
        //
        // GET: /introspect/
        private QuestionService _questionService;
        QuestionService QuestionService
        {
            get
            {
                if (HttpContext.User.Identity.Name != "ljcwyc")
                {
                    throw new Exception();
                }
                if (_questionService == null)
                {
                    return _questionService = new QuestionService
                    {
                        CurrentUser = new MyUserService().GetByUserName(HttpContext.User.Identity.Name)
                    };
                }
                return _questionService;
            }
        }

        public ActionResult Index()
        {
            var list = QuestionService.GetList();
            return View(list);
        }

        public ActionResult Search(string likeStr = null)
        {
            var list = new List<question>();
            if (!string.IsNullOrWhiteSpace(likeStr))
            {
                list = QuestionService.Search(likeStr);
            }
            return View(list);
        }

        //
        // GET: /question/Details/5

        public ActionResult Details(string id)
        {
            var entity = QuestionService.Get(id);
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
                    if (QuestionService.Add(entity))
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
                if (!QuestionService.DeleteAll()) return "清空所有问题失败！";

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
            var entity = QuestionService.Get(id);
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
                    if (QuestionService.Update(entity))
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
            var word = QuestionService.Get(id);
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
                if (QuestionService.Delete(id))
                {
                    return RedirectToAction("Index");
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
                QuestionService.Add(entity);
            }

        }

        public IActionResult QuickAdd(question model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult QuickAdd([FromServices]IHostingEnvironment env, question questionModel)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(questionModel.QuestionMember))
                {
                    return View();
                }
                if (QuestionService.IsQuestionExist(questionModel.QuestionMember.Trim()))
                {
                    return RedirectToAction(nameof(QuickAdd), new question() {QuestionMember = "问题已存在"});
                }
                questionModel.FullScore = 10;
                if (QuestionService.Add(questionModel))
                {
                    return RedirectToAction(nameof(QuickAdd), new question());
                }
                return RedirectToAction(nameof(QuickAdd), new question() { QuestionMember = "新增失败" });
            }
            return View();
        }


    }
}
