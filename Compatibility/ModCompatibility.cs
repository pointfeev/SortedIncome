using HarmonyLib;
using System;

namespace SortedIncome
{
    public static class ModCompatibility
    {
        public static void PatchAll(Harmony harmony)
        {
            foreach (Type type in typeof(ModCompatibility).Assembly.GetTypes())
                if (type.Namespace == "SortedIncome.Compatibility.Mods")
                    type.GetMethod("Patch")?.Invoke(null, new object[] { harmony });
        }
    }
}