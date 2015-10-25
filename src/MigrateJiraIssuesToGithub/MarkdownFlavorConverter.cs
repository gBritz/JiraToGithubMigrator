using System;
using System.Text.RegularExpressions;

namespace MigrateJiraIssuesToGithub
{
    public class MarkdownFlavorConverter
    {
        public String ReplaceFromJiraToGithub(String content)
        {
            if (String.IsNullOrEmpty(content))
            {
                return content;
            }

            var replaces = new Func<String, String>[]
            {
                ReplaceJiraMentionsToGithub
            };

            foreach (var replacer in replaces)
            {
                content = replacer(content);
            }

            return content;
        }

        protected String ReplaceJiraMentionsToGithub(String text)
        {
            return Regex.Replace(text, @"(\[\~)(\w+)(\])", "@$2");
        }
    }
}