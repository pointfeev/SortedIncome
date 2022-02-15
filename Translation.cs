
using TaleWorlds.Localization;

namespace SortedIncome
{
    internal static class Translation
    {
        internal const string ModPrefix = "si_";

        internal static string GetTranslationID(this string s) => ModPrefix
            + s.ToLower().Trim().Replace(" ", "_")
            .Replace("&", "and").Replace("'", "")
            .Replace("(", "LP").Replace(")", "RP")
            .Replace("[", "LB").Replace("]", "RB")
            .Replace("{", "LSB").Replace("}", "RSB");

        internal static string Translate(this string s) => new TextObject($"{{={s.GetTranslationID()}}}" + s).ToString();

        internal static bool TranslatedContains(this string s1, string s2) => s1.Translate().Contains(s2.Translate());
    }
}
