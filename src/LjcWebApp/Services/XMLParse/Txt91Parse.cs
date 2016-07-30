using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LjcWebApp;

namespace LjcWebApp.Services.XMLParse
{
    public class Txt91Parse
    {
        public List<word_tb> Split91LexiconToWords(string filename,string classs, int priority)
        {
            FileStream rFile = new FileStream(filename, FileMode.Open);
            StreamReader sr = new StreamReader(rFile, Encoding.GetEncoding("gb2312"));//读取中文简体编码GB2312

            string str = sr.ReadToEnd();
            sr.Dispose();
            int j = str.IndexOf('\n');
            while (j != -1)
            {
                str = str.Remove(j, 1);
                j = str.IndexOf('\n');

            }

            string[] split = str.Split(new char[] { '\r' });

            int length = split.Length;
            for (int i = 1; i < length - 1; i++)//如果split数组中有一行是空,将它的上下两行合并,数组长度减2，若头尾两行为空,干脆舍去不要(不再合并)
            {
                if (split[i] == "")
                {
                    split[i - 1] = split[i - 1] + split[i + 1];
                    for (int q = i; q < length - 2; q++)
                        split[q] = split[q + 2];
                    length = length - 2;
                }
            }

            List<word_tb> list = new List<word_tb>();
            int a, b;
            for (int i = 0; i < length - 1; i++)
            {
                string spelling = "", soundmark = "", meaning = "";
                a = split[i].IndexOf('[');
                b = split[i].IndexOf(']');

                if (a == -1 || b == -1) continue;//若改行没有左右中括号,舍去
                spelling = split[i].Substring(0, a).Trim();
                soundmark = split[i].Substring(a, b - a + 1).Trim();
                meaning = split[i].Substring(b + 1).Trim();

                //遇到一条解释时，说明一个单词的内容已经取完，可以加入集合
                var wordTb = new word_tb();
                wordTb.Spelling = spelling;
                wordTb.Phonetic = soundmark;
                wordTb.Paraphrase = meaning;

                int index = wordTb.Paraphrase.ToLower().IndexOf(wordTb.Spelling.ToLower());
                while (index != -1)
                {
                    wordTb.Paraphrase = wordTb.Paraphrase.Substring(0, index)
                                        + "~" + wordTb.Paraphrase.Substring(index + wordTb.Spelling.Length);
                    index = wordTb.Paraphrase.ToLower().IndexOf(wordTb.Spelling.ToLower());
                }

                wordTb.Classs = classs;

                wordTb.Import = DateTime.Now;
                wordTb.CreatedOn = wordTb.Import;
                wordTb.ModifiedOn = DateTime.MinValue;//导入单词时ModifiedOn设为最小值，防止跟记忆时ModifiedOn的作用发生混淆

                wordTb.Priority = priority;
                //默认进度为0
                wordTb.Process = 0;
                list.Add(wordTb);
            }
            return list;
        }
    }
}
