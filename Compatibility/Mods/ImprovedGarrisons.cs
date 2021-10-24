using HarmonyLib;
using System;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace SortedIncome.Compatibility.Mods
{
    public static class ImprovedGarrisonsMod
    {
        public static bool IsActive => (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                        from type in asm.GetTypes()
                                        let ns = type.Namespace?.Contains("ImprovedGarrisons")
                                        where ns.GetValueOrDefault(false)
                                        select type).Any();

        public static void Patch(Harmony harmony)
        {
            if (IsActive)
            {
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("ImprovedGarrisons.Models.GarrisonCostModel"), "CalculateImprovedGarrisonCosts"),
                    postfix: new HarmonyMethod(typeof(ImprovedGarrisonsMod), "CalculateImprovedGarrisonCosts")
                );
            }
        }

        public static void CalculateImprovedGarrisonCosts(ref ExplainedNumber __result, bool includeDescriptions)
        {
            __result = Sorting.Sort(__result, includeDescriptions);
        }
    }
}