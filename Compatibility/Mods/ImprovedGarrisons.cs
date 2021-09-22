using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace SortedIncome.Mods
{
    public static class ImprovedGarrisonsMod
    {
        public static bool IsActive = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                                       from type in asm.GetTypes()
                                       where type.Namespace == "ImprovedGarrisons"
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

        public static void CalculateImprovedGarrisonCosts(ref ExplainedNumber __result, bool includeDescriptions = false)
        {
            __result = Sorting.Sort(__result, includeDescriptions);
        }

        public static void CustomSort(List<ValueTuple<string, float>> originalList, List<ValueTuple<string, float>> sortedList)
        {
            for (int i = originalList.Count - 1; i >= 0; i--)
            {
                int sortedIndex = sortedList.Count;
                ValueTuple<string, float> tuple = originalList[i];
                string name = tuple.Item1;
                if (name.Contains("Improved Garrison"))
                {
                    name = Sorting.GetIncrementedName(sortedIndex, "Garrison recruitment", countPrefix: "for ", countSuffix: new Tuple<string, string>(" settlement", " settlements"));
                }
                else if (name.Contains("Garrisonguards wages"))
                {
                    name = Sorting.GetIncrementedName(sortedIndex, "Garrisonguard wages", countSuffix: new Tuple<string, string>(" garrisonguard", " garrisonguards"));
                }
                else if (name.Contains("costs"))
                {
                    name = Sorting.GetIncrementedName(sortedIndex, "Garrison recruiter wages", countSuffix: new Tuple<string, string>(" garrison recruiter", " garrison recruiters"));
                }
                else if (name.Contains("finance help"))
                {
                    name = Sorting.GetIncrementedName(sortedIndex, "Garrison financial help", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrison", " garrisons"));
                }
                else
                {
                    continue;
                }
                sortedList.Insert(sortedIndex, new ValueTuple<string, float>(name, tuple.Item2));
                originalList.RemoveAt(i);
            }
        }
    }
}