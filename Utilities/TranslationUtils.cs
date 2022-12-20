using TaleWorlds.Localization;

namespace SortedIncome.Utilities
{
    internal static class TranslationUtils
    {
        private const string ModPrefix = "si_";

        private static string GetDynamicTranslationId(this string s) => ModPrefix
                                                                      + s.ToLower().Trim().Replace(" ", "_")
                                                                         .Replace("&", "and").Replace("'", "")
                                                                         .Replace("(", "LP").Replace(")", "RP")
                                                                         .Replace("[", "LB").Replace("]", "RB")
                                                                         .Replace("{", "LSB").Replace("}", "RSB");

        internal static string TranslateWithDynamicId(this string s)
            => new TextObject($"{{={s.GetDynamicTranslationId()}}}" + s).ToString();

        private static string TranslateId(this string id)
            => LocalizedTextManager.GetTranslatedText(MBTextManager.ActiveTextLanguage, id) ?? string.Empty;

        internal static bool Parse(this string name, string contains, string translationTemplate = null)
        {
            if (name.Contains(contains))
                return true;
            if (translationTemplate is null)
                return false;
            string translation = "";
            string currentId = "";
            bool open = false;
            char[] chars = translationTemplate.ToCharArray();
            foreach (char c in chars)
                switch (c)
                {
                    case '{':
                        open = true;
                        break;
                    case '}':
                    {
                        bool _open = false;
                        char[] _chars = TranslateId(currentId).ToCharArray();
                        foreach (char _c in _chars)
                            switch (_c)
                            {
                                case '{':
                                    _open = true;
                                    break;
                                case '}':
                                    _open = false;
                                    break;
                                default:
                                {
                                    if (!_open)
                                        translation += _c;
                                    break;
                                }
                            }
                        currentId = "";
                        open = false;
                        break;
                    }
                    default:
                    {
                        if (open)
                            currentId += c;
                        else
                            translation += c;
                        break;
                    }
                }
            if (string.IsNullOrWhiteSpace(translation))
                return false;
            if (translation[translation.Length - 1] == ')')
                translation = translation.Substring(0, translation.Length - 1);
            //InformationManager.DisplayMessage(new InformationMessage("'" + translationTemplate + "' => '" + translation + "'", Colors.Yellow, "SortedIncome"));
            return name.Contains(translation);
        }
    }
}