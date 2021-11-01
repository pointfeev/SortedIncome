using HarmonyLib;
using System;
using System.Linq;

namespace SortedIncome.Compatibility.Mods
{
    internal static class ImprovedGarrisonsMod
    {
        internal static bool IsActive => (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                          from type in asm.GetTypes()
                                          let ns = type.Namespace?.Contains("ImprovedGarrisons")
                                          where ns.GetValueOrDefault(false)
                                          select type).Any();

        internal static void Patch(Harmony harmony)
        {
            if (IsActive)
            {
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("ImprovedGarrisons.Models.GarrisonCostModel"), "CalculateClanExpenses"),
                    postfix: new HarmonyMethod(typeof(DefaultClanFinanceModelPatch), "Calculate")
                );
                harmony.Patch(
                    original: AccessTools.Method(AccessTools.TypeByName("ImprovedGarrisons.Models.GarrisonCostModel"), "CalculateClanGoldChange"),
                    postfix: new HarmonyMethod(typeof(DefaultClanFinanceModelPatch), "Calculate")
                );
            }
        }
    }
}