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
    }
}