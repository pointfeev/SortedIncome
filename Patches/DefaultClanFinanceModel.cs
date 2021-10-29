using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace SortedIncome
{
    [HarmonyPatch(typeof(DefaultClanFinanceModel))]
    public static class DefaultClanFinanceModelPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("CalculateClanGoldChange")]
        [HarmonyPatch("CalculateClanIncome")]
        [HarmonyPatch("CalculateClanExpenses")]
        public static void CalculateClanGoldChange(ref ExplainedNumber __result, bool includeDescriptions) => __result = Sorting.Sort(__result, includeDescriptions);
    }
}