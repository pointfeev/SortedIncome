using TaleWorlds.Localization;

namespace SortedIncome
{
    internal static class TranslationUtils
    {
        internal const string ModPrefix = "si_";

        internal static string GetDynamicTranslationID(this string s) => ModPrefix
            + s.ToLower().Trim().Replace(" ", "_")
            .Replace("&", "and").Replace("'", "")
            .Replace("(", "LP").Replace(")", "RP")
            .Replace("[", "LB").Replace("]", "RB")
            .Replace("{", "LSB").Replace("}", "RSB");

        internal static string TranslateWithDynamicID(this string s) => new TextObject($"{{={s.GetDynamicTranslationID()}}}" + s).ToString();

        internal static string TranslateID(this string id) => LocalizedTextManager.GetTranslatedText(MBTextManager.ActiveLanguage, id) ?? string.Empty;

        internal static bool Parse(this string name, string contains, string translationTemplate = null)
        {
            if (name.Contains(contains)) return true;
            else if (!(translationTemplate is null))
            {
                string translation = "";
                string currentID = "";
                bool open = false;
                char[] chars = translationTemplate.ToCharArray();
                foreach (char c in chars)
                    if (c == '{')
                        open = true;
                    else if (c == '}')
                    {
                        bool _open = false;
                        char[] _chars = TranslateID(currentID).ToCharArray();
                        foreach (char _c in _chars)
                        {
                            if (_c == '{')
                                _open = true;
                            else if (_c == '}')
                                _open = false;
                            else if (!_open)
                                translation += _c;
                        }
                        currentID = "";
                        open = false;
                    }
                    else if (open)
                        currentID += c;
                    else
                        translation += c;
                if (string.IsNullOrWhiteSpace(translation)) return false;
                if (translation[translation.Length - 1] == ')') translation = translation.Substring(0, translation.Length - 1);
                //InformationManager.DisplayMessage(new InformationMessage("'" + translationTemplate + "' => '" + translation + "'", Colors.Yellow, "SortedIncome"));
                return name.Contains(translation);
            }
            return false;
        }
    }
}
