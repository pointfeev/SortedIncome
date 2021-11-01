using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace SortedIncome
{
    [HarmonyPatch(typeof(DefaultClanFinanceModel))]
    internal static class DefaultClanFinanceModelPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CalculateClanGoldChange")]
        [HarmonyPatch("CalculateClanIncome")]
        [HarmonyPatch("CalculateClanExpenses")]
        internal static void Calculate(ref ExplainedNumber __result, bool includeDescriptions) => __result = Sorting.Sort(__result, includeDescriptions);
    }
}