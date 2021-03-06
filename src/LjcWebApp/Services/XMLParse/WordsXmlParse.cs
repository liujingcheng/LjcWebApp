﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using LjcWebApp;
using LjcWebApp.Helper;


namespace LjcWebApp.Services.XMLParse
{
    public class WordsXmlParse
    {
        /// <summary>
        /// 文件
        /// </summary>
        public string XmlFile { get; set; }

        /// <summary>
        /// 路径
        /// </summary>
        public XDocument Document { get; set; }

        public WordsXmlParse()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="xmlPath">XML文件</param>
        public WordsXmlParse(string xmlPath)
        {
            XmlFile = xmlPath;
            try
            {
                if (File.Exists(XmlFile))
                {
                    Document = XDocument.Load(XmlFile);
                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("XDocument.Load(XmlFile)加载文件失败", e);
            }
        }

        public List<word_tb> DoParse(int priority)
        {
            var list = new List<word_tb>();

            if (Document == null)
            {
                LogHelper.WriteLog("Document为null");
                return null;
            }

            var wordbook = Document.Element("wordbook");
            if (wordbook == null)
            {
                LogHelper.WriteLog("XML找不到“wordbook”节点");
                return null;
            }

            var items = wordbook.Elements("item");
            try
            {
                foreach (var item in items)
                {
                    var wordTb = ToWordTb(item, priority);
                    if (wordTb != null)
                    {
                        list.Add(wordTb);
                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("ToWordTb(item)方法异常", e);
                return null;
            }

            return list;
        }

        private word_tb ToWordTb(XElement item, int priority)
        {
            var wordTb = new word_tb();

            XElement word = item.Element("word");
            if (word == null) return null;
            wordTb.Spelling = word.Value;

            XElement trans = item.Element("trans");
            if (trans != null) //解释里面有单词时，以波浪线代替
            {
                wordTb.Paraphrase = trans.Value;
                int index = wordTb.Paraphrase.ToLower().IndexOf(wordTb.Spelling.ToLower());
                while (index != -1)
                {
                    wordTb.Paraphrase = wordTb.Paraphrase.Substring(0, index)
                                        + "~" + wordTb.Paraphrase.Substring(index + wordTb.Spelling.Length);
                    index = wordTb.Paraphrase.ToLower().IndexOf(wordTb.Spelling.ToLower());
                }
            }

            XElement phonetic = item.Element("phonetic");
            if (phonetic != null)
                wordTb.Phonetic = phonetic.Value;

            XElement tags = item.Element("tags");
            wordTb.Classs = tags != null ? tags.Value : "未分类";//如果分类为空的话就赋值为“未分类”

            wordTb.Import = DateTime.Now;
            wordTb.CreatedOn = wordTb.Import;
            wordTb.ModifiedOn = DateTime.MinValue;//导入单词时ModifiedOn设为最小值，防止跟记忆时ModifiedOn的作用发生混淆

            wordTb.Priority = priority;
            //默认进度为0
            wordTb.Process = 0;

            return wordTb;
        }
    }
}
