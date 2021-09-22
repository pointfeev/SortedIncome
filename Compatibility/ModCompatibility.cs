using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SortedIncome
{
    public static class ModCompatibility
    {
        private static void RunMethodOnAll(string methodName, params object[] parameters)
        {
            foreach (Type type in typeof(ModCompatibility).Assembly.GetTypes())
            {
                if (type.Namespace == "SortedIncome.Mods")
                {
                    MethodInfo method = type.GetMethod(methodName);
                    if (!(method is null))
                    {
                        method.Invoke(null, parameters);
                    }
                }
            }
        }

        public static void PatchAll(Harmony harmony)
        {
            RunMethodOnAll("Patch", new object[] { harmony });
        }

        public static void CustomSortAll(List<ValueTuple<string, float>> originalList, List<ValueTuple<string, float>> sortedList)
        {
            RunMethodOnAll("CustomSort", new object[] { originalList, sortedList });
        }
    }
}