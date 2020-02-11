using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace newsreaderui
{
    /// <summary>
    /// An example model program.
    /// </summary>
    static class NewsreaderModelProgram
    {
        public enum Page { Topics, Messages };
        public enum Style { WithText, TitlesOnly };
        public enum Sort { ByFirst, ByMostRecent };

        public static Page page = Page.Topics;
        public static Style style = Style.WithText;
        public static Sort sort = Sort.ByMostRecent;

        [Probe]
        static string Show()
        {
            return String.Format("{0}, {1}, {2}:", page, style, sort);
        }

        [Rule(Action = "SelectMessages()")]
        public static void SelectMessages()
        {
            Condition.IsTrue(page == Page.Topics);
            page = Page.Messages;
        }

        [Rule(Action = "SelectTopics()")]
        public static void SelectTopics()
        {
            Condition.IsTrue(page == Page.Messages);
            page = Page.Topics;
        }

        [Rule(Action = "ShowTitles()")]
        public static void ShowTitles()
        {
            Condition.IsTrue(page == Page.Topics && style == Style.WithText);
            style = Style.TitlesOnly;
        }

        [Rule(Action = "ShowText()")]
        public static void ShowText()
        {
            Condition.IsTrue(page == Page.Topics && style == Style.TitlesOnly);
            style = Style.WithText;
        }

        [Rule(Action = "SortByFirst()")]
        public static void SortByFirst()
        {
            Condition.IsTrue(page == Page.Topics && style == Style.TitlesOnly
                && sort == Sort.ByMostRecent);
            sort = Sort.ByFirst;

        }

        [Rule(Action = "SortByMostRecent()")]
        public static void SortByMostRecent()
        {
            Condition.IsTrue(page == Page.Topics && style == Style.TitlesOnly
                && sort == Sort.ByFirst);
            sort = Sort.ByMostRecent;
        }


        

    }
}
