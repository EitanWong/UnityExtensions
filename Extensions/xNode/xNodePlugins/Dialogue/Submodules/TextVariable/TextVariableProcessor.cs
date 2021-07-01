using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dialogue.Scripts.Utilities;
using UnityEditor;
using UnityEngine;

namespace Dialogue.Submodules.TextVariable
{
    public static class TextVariableProcessor
    {

        private static List<TextVariable> _variables;

        public static string ProcessVariable(string allText)
        {
            if (_variables == null)
                _variables = DialogueInternalUtility.GetAllReflectionClassIns<TextVariable>();

            var tokens= GetTokens(allText);

            foreach (var token in tokens)
            {
                foreach (var v in _variables)
                {
                    if (v.Detect(token))
                    {
                        allText= allText.Replace(string.Format("{{{0}}}",token), v.Process(token));
                        break;
                    }
                }
            }
            return allText;
        }
        private static List<string> GetTokens(string str)
        {
            Regex regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(str);

            // Results include braces (undesirable)
            return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }
    }
}