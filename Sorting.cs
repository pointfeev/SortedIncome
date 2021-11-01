using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;

namespace SortedIncome
{
    public static class Sorting
    {
        public static ExplainedNumber Sort(ExplainedNumber result, bool includeDescriptions)
        {
            if (InputKey.LeftAlt.IsDown()) return result;
            try
            {
                ExplainedNumber sortedGoldChange = new ExplainedNumber(includeDescriptions: includeDescriptions);
                Sort(result, ref sortedGoldChange);
                sortedGoldChange.LimitMin(result.ResultNumber); // these 2 lines are to make sure income
                sortedGoldChange.LimitMax(result.ResultNumber); // gained each day isn't actually changed
                return sortedGoldChange;
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

        public static string GetIncrementedName(int indexInSortedList, string name, bool countAsIncrement = true, string countPrefix = "", Tuple<string, string> countSuffix = null)
        {
            if (countSuffix is null)
            {
                countSuffix = new Tuple<string, string>("", "");
            }
            if (stringMentions.TryGetValue(name, out List<Tuple<int, bool>> sortedListIndexes))
            {
                int count = sortedListIndexes.FindAll(tuple => tuple.Item2).Count + (countAsIncrement ? 1 : 0);
                string incrementedName = name + (count > 0 ? $" ({countPrefix}{count}{(count == 1 ? countSuffix.Item1 : countSuffix.Item2)})" : "");
                foreach (Tuple<int, bool> tuple in sortedListIndexes)
                {
                    sortedList[tuple.Item1] = new ValueTuple<string, float>(incrementedName, sortedList[tuple.Item1].Item2);
                }
                stringMentions[name].Add(new Tuple<int, bool>(indexInSortedList, countAsIncrement));
                return incrementedName;
            }
            stringMentions[name] = new List<Tuple<int, bool>>() { new Tuple<int, bool>(indexInSortedList, countAsIncrement) };
            return name + (countAsIncrement ? $" ({countPrefix}{1}{countSuffix.Item1})" : "");
        }

        private static Settlement GetSettlementFromName(string name, out Settlement settlement)
        {
            MBReadOnlyList<Settlement> settlements = Campaign.Current.Settlements;
            foreach (Settlement _settlement in settlements)
            {
                if (_settlement.Name.ToString() == name)
                {
                    settlement = _settlement;
                    return settlement;
                }
            }
            settlement = null;
            return settlement;
        }

        private static void Sort(ExplainedNumber originalGoldChange, ref ExplainedNumber sortedGoldChange)
        {
            List<ValueTuple<string, float>> originalList = originalGoldChange.GetLines();
            for (int i = originalList.Count - 1; i >= 0; i--)
            {
                int sortedIndex = sortedList.Count;
                ValueTuple<string, float> tuple = originalList[i];
                string name = tuple.Item1;
                if (name.Contains("Party wages Garrison of "))
                    name = GetIncrementedName(sortedIndex, "Garrison wages", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrison", " garrisons"));
                else if (name.Contains("Party wages "))
                    name = GetIncrementedName(sortedIndex, "Party wages", countPrefix: "for ", countSuffix: new Tuple<string, string>(" party", " parties"));
                else if (name.Contains("Caravan ("))
                    name = GetIncrementedName(sortedIndex, "Caravan balance", countPrefix: "from ", countSuffix: new Tuple<string, string>(" caravan", " caravans"));
                else if (name.Contains("'s tariff"))
                    name = GetIncrementedName(sortedIndex, "Town tax & tariffs", countPrefix: "from ", countSuffix: new Tuple<string, string>(" town", " towns"));
                else if (name.Contains("Tribute from "))
                    name = GetIncrementedName(sortedIndex, "Tribute", countPrefix: "from ", countSuffix: new Tuple<string, string>(" kingdom", " kingdoms"));
                else if (!(GetSettlementFromName(name, out Settlement settlement) is null) && settlement.IsVillage)
                    name = GetIncrementedName(sortedIndex, "Village tax", countPrefix: "from ", countSuffix: new Tuple<string, string>(" village", " villages"));
                else if (!(settlement is null) && settlement.IsCastle)
                    name = GetIncrementedName(sortedIndex, "Castle tax", countPrefix: "from ", countSuffix: new Tuple<string, string>(" castle", " castles"));
                else if (!(settlement is null) && settlement.IsTown)
                    name = GetIncrementedName(sortedIndex, "Town tax & tariffs", countAsIncrement: false, countPrefix: "from ", countSuffix: new Tuple<string, string>(" town", " towns"));
                else if (name.Contains("Improved Garrison Training of "))
                    name = GetIncrementedName(sortedIndex, "Garrison training", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrison", " garrisons"));
                else if (name.Contains("Garrisonguards wages"))
                    name = GetIncrementedName(sortedIndex, "Garrisonguard wages", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrisonguard", " garrisonguards"));
                else if (name.Contains("costs"))
                    name = GetIncrementedName(sortedIndex, "Garrison recruitment", countPrefix: "for ", countSuffix: new Tuple<string, string>(" recruiter", " recruiters"));
                else if (name.Contains("finance help"))
                    name = GetIncrementedName(sortedIndex, "Garrison financial help", countPrefix: "for ", countSuffix: new Tuple<string, string>(" garrison", " garrisons"));
                sortedList.Insert(sortedIndex, new ValueTuple<string, float>(name, tuple.Item2));
                originalList.RemoveAt(i);
            }
            sortedList.Reverse();
            for (int i = 0; i < sortedList.Count; i++)
            {
                ValueTuple<string, float> tuple = sortedList[i];
                sortedGoldChange.Add((int)tuple.Item2, tuple.Item1.AsTextObject());
            }
            stringMentions.Clear();
            sortedList.Clear();
        }
    }
}