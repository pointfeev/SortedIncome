using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace SortedIncome
{
    internal static class Sorting
    {
        internal static void SorterPatchDenars(ref ExplainedNumber __result, bool includeDescriptions) => __result = Sort(__result, includeDescriptions, "Denars");
        internal static void SorterPatchInfluence(ref ExplainedNumber __result, bool includeDescriptions) => __result = Sort(__result, includeDescriptions, "Influence");

        private static ExplainedNumber Sort(ExplainedNumber result, bool includeDescriptions, string form)
        {
            if (InputKey.LeftAlt.IsDown()) return result;
            try
            {
                ExplainedNumber sortedChange = new ExplainedNumber(includeDescriptions: includeDescriptions);
                Sort(result, ref sortedChange, form);
                return sortedChange;
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
            return result;
        }

        //                                   name, sorted list indexes
        private static readonly Dictionary<string, List<Tuple<int, bool>>> stringMentions = new Dictionary<string, List<Tuple<int, bool>>>();

        private static readonly List<ValueTuple<string, float>> sortedList = new List<ValueTuple<string, float>>();

        internal static string GetIncrementedName(int indexInSortedList, string name, bool countAsIncrement = true, string countPrefix = "", Tuple<string, string> countSuffix = null)
        {
            if (countSuffix is null) countSuffix = new Tuple<string, string>("", "");
            if (stringMentions.TryGetValue(name, out List<Tuple<int, bool>> sortedListIndexes))
            {
                int count = sortedListIndexes.FindAll(tuple => tuple.Item2).Count + (countAsIncrement ? 1 : 0);
                string incrementedName = name + (count > 0 ? $" ({countPrefix}{count}{(count == 1 ? countSuffix.Item1 : countSuffix.Item2)})" : "");
                foreach (Tuple<int, bool> tuple in sortedListIndexes)
                    sortedList[tuple.Item1] = new ValueTuple<string, float>(incrementedName, sortedList[tuple.Item1].Item2);
                stringMentions[name].Add(new Tuple<int, bool>(indexInSortedList, countAsIncrement));
                return incrementedName;
            }
            stringMentions[name] = new List<Tuple<int, bool>>() { new Tuple<int, bool>(indexInSortedList, countAsIncrement) };
            return name + (countAsIncrement ? $" ({countPrefix}{1}{countSuffix.Item1})" : "");
        }

        private static bool TryGetSettlementFromName(string name, out Settlement settlement)
        {
            MBReadOnlyList<Settlement> settlements = Campaign.Current.Settlements;
            foreach (Settlement _settlement in settlements)
            {
                if (_settlement.Name.ToString() == name)
                {
                    settlement = _settlement;
                    return true;
                }
            }
            settlement = null;
            return false;
        }

        private static readonly Dictionary<string, PolicyObject> defaultPolicyCache = new Dictionary<string, PolicyObject>();
        private static bool TryGetKingdomPolicyFromName(string name, out PolicyObject policy)
        {
            if (defaultPolicyCache.TryGetValue(name, out PolicyObject _policy))
            {
                policy = _policy;
                return true;
            }
            else
            {
                foreach (PropertyInfo property in typeof(DefaultPolicies).GetProperties(BindingFlags.Public | BindingFlags.Static))
                    if (property.PropertyType == typeof(PolicyObject))
                    {
                        PolicyObject policyObject = (PolicyObject)property.GetValue(null, null);
                        if (!(policyObject is null) && policyObject.Name.ToString() == name)
                        {
                            defaultPolicyCache.Add(name, policyObject);
                            policy = policyObject;
                            return true;
                        }
                    }
            }
            policy = null;
            return false;
        }

        private static void Sort(ExplainedNumber originalChange, ref ExplainedNumber sortedChange, string form)
        {
            List<ValueTuple<string, float>> originalList = originalChange.GetLines();
            for (int i = originalList.Count - 1; i >= 0; i--)
            {
                int sortedIndex = sortedList.Count;
                ValueTuple<string, float> tuple = originalList[i];
                string name = tuple.Item1;
                if (form == "Denars")
                {
                    if (TryGetKingdomPolicyFromName(name, out _))
                        name = GetIncrementedName(sortedIndex, "Kingdom policies", countPrefix: "from ", countSuffix: new Tuple<string, string>(" policy", " policies"));
                    else if (name.Contains("Party wages Garrison of "))
                        name = GetIncrementedName(sortedIndex, "Garrison wages", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrison", " garrisons"));
                    else if (name.Contains("Party wages "))
                        name = GetIncrementedName(sortedIndex, "Party wages", countPrefix: "for ", countSuffix: new Tuple<string, string>(" party", " parties"));
                    else if (name.Contains("Caravan ("))
                        name = GetIncrementedName(sortedIndex, "Caravan balance", countPrefix: "from ", countSuffix: new Tuple<string, string>(" caravan", " caravans"));
                    else if (name.Contains("'s tariff"))
                        name = GetIncrementedName(sortedIndex, "Town tax & tariffs", countPrefix: "from ", countSuffix: new Tuple<string, string>(" town", " towns"));
                    else if (name.Contains("Tribute from "))
                        name = GetIncrementedName(sortedIndex, "Tribute", countPrefix: "from ", countSuffix: new Tuple<string, string>(" kingdom", " kingdoms"));
                    else if (TryGetSettlementFromName(name, out Settlement settlement) && settlement.IsVillage)
                        name = GetIncrementedName(sortedIndex, "Village tax", countPrefix: "from ", countSuffix: new Tuple<string, string>(" village", " villages"));
                    else if (!(settlement is null) && settlement.IsCastle)
                        name = GetIncrementedName(sortedIndex, "Castle tax", countPrefix: "from ", countSuffix: new Tuple<string, string>(" castle", " castles"));
                    else if (!(settlement is null) && settlement.IsTown)
                        name = GetIncrementedName(sortedIndex, "Town tax & tariffs", countAsIncrement: false, countPrefix: "from ", countSuffix: new Tuple<string, string>(" town", " towns"));
                    // Improved Garrisons support
                    else if (name.Contains("Improved Garrison Training of "))
                        name = GetIncrementedName(sortedIndex, "Garrison training", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrison", " garrisons"));
                    else if (name.Contains("Garrisonguards wages"))
                        name = GetIncrementedName(sortedIndex, "Garrisonguard wages", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrisonguard", " garrisonguards"));
                    else if (name.Contains("costs"))
                        name = GetIncrementedName(sortedIndex, "Garrison recruitment", countPrefix: "for ", countSuffix: new Tuple<string, string>(" recruiter", " recruiters"));
                    else if (name.Contains("finance help"))
                        name = GetIncrementedName(sortedIndex, "Garrison financial help", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrison", " garrisons"));
                }
                else if (form == "Influence")
                {
                    if (TryGetKingdomPolicyFromName(name, out _))
                        name = GetIncrementedName(sortedIndex, "Kingdom policies", countPrefix: "from ", countSuffix: new Tuple<string, string>(" policy", " policies"));
                    // Population of Calradia support
                    else if (name.Contains("Nobles influence from "))
                        name = GetIncrementedName(sortedIndex, "Nobles influence", countPrefix: "from ", countSuffix: new Tuple<string, string>(" settlement", " settlements"));
                    else if (name.Contains("Population growth policy at "))
                        name = GetIncrementedName(sortedIndex, "Population growth policies", countPrefix: "at ", countSuffix: new Tuple<string, string>(" settlement", " settlements"));
                }

                sortedList.Insert(sortedIndex, new ValueTuple<string, float>(name, tuple.Item2));
                originalList.RemoveAt(i);
            }
            sortedList.Reverse();
            foreach (ValueTuple<string, float> tuple in sortedList)
                sortedChange.Add(tuple.Item2, tuple.Item1.AsTextObject());
            stringMentions.Clear();
            sortedList.Clear();
        }
    }
}