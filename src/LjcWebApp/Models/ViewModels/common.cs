using System;
using System.Collections.Generic;
using System.Linq;

namespace LjcWebApp.Models.ViewModels
{
    public class common
    {
        public static List<word_tb> AllWordsToReview;//所有要复习的单词

        public static LinkedList<word_tb> LinkedList;
        public static LinkedListNode<word_tb> CurrentNode;
        public static List<word_tb> WordsRemembered;
        public static List<word_tb> WordsNotRemember;

        public static string lblRemainText;
        public static string lblSpellingText;
        public static string lblPhoneticText;
        public static string lblParaphraseText;
        public static string lblWordInfoText;

        public static bool rbHideRandomChecked;
        public static bool rbHideSpellingChecked;
        public static bool rbHideParaphraseChecked;

        public static bool BtnNOEnabled;
        public static bool BtnYESEnabled;
        public static bool BtnPreviousEnabled;
        public static bool BtnNextEnabled;
        public static bool BtnDeleteEnabled;

        public static bool IsFirstNo;//标识首次点击No，此时只展示全部单词信息，不切换单词
    }
}