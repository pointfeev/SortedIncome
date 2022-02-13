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
        internal static void Patch(ref List<(string name, float number)> __result)
        {
            if (InputKey.LeftAlt.IsDown()) return;
            __result = Sort(__result);
        }

        private static readonly Dictionary<string, (string prefix, (string singular, string plural) suffix)> countStrings
            = new Dictionary<string, (string prefix, (string singular, string plural) suffix)>();

        private static readonly Dictionary<string, (float number, int mentions)> lines = new Dictionary<string, (float number, int mentions)>();

        private static List<(string name, float number)> Sort(List<(string name, float number)> __result)
        {
            try
            {
                countStrings.Clear();
                lines.Clear();
                List<(string name, float number)> result = new List<(string name, float number)>();
                foreach ((string _name, float number) in __result)
                {
                    bool incrementMentions = true;
                    string name = _name;
                    if (TryGetKingdomPolicyFromName(name, out _))
                        name = SetupSorting("Kingdom policies", "from ", (" policy", " policies"));
                    else if (name.Contains("Party wages Garrison of "))
                        name = SetupSorting("Garrison wages", "for ", (" garrison", " garrisons"));
                    else if (name.Contains("Party wages "))
                        name = SetupSorting("Party wages", "for ", (" party", " parties"));
                    else if (name.Contains("Caravan ("))
                        name = SetupSorting("Caravan balance", "from ", (" caravan", " caravans"));
                    else if (name.Contains("Tribute from "))
                        name = SetupSorting("Tribute", "from ", (" kingdom", " kingdoms"));
                    else if (TryGetSettlementFromName(name, out Settlement settlement) && settlement.IsVillage)
                        name = SetupSorting("Village tax", "from ", (" village", " villages"));
                    else if (!(settlement is null) && settlement.IsCastle)
                        name = SetupSorting("Castle tax", "from ", (" castle", " castles"));
                    else if (name.Contains("'s tariff") || !(settlement is null) && settlement.IsTown)
                    {
                        name = SetupSorting("Town tax & tariffs", "from ", (" town", " towns"));
                        if (!(settlement is null) && settlement.IsTown) incrementMentions = false;
                    }
                    // Improved Garrisons support
                    else if (name.Contains("Improved Garrison Training of "))
                        name = SetupSorting("Garrison training", "for ", (" garrison", " garrisons"));
                    else if (name.Contains("Garrisonguards wages"))
                        name = SetupSorting("Garrisonguard wages", "for ", (" garrisonguard", " garrisonguards"));
                    else if (name.Contains("costs"))
                        name = SetupSorting("Garrison recruitment", "for ", (" recruiter", " recruiters"));
                    else if (name.Contains("finance help"))
                        name = SetupSorting("Garrison financial help", "for ", (" garrison", " garrisons"));
                    // Population of Calradia support
                    else if (name.Contains("Nobles influence from "))
                        name = SetupSorting("Nobles influence", "from ", (" settlement", " settlements"));
                    else if (name.Contains("Population growth policy at "))
                        name = SetupSorting("Population growth policies", "at ", (" settlement", " settlements"));
                    int increment = incrementMentions ? 1 : 0;
                    lines[name] = lines.ContainsKey(name) ? (lines[name].number + number, lines[name].mentions + increment) : (number, increment);
                }
                foreach (KeyValuePair<string, (float number, int mentions)> line in lines)
                    result.Add((GetFinalName(line.Key, line.Value.mentions), line.Value.number));
                return result;
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
            return __result;
        }

        internal static string SetupSorting(string name, string countPrefix, (string singular, string plural) countSuffix)
        {
            if (!countStrings.ContainsKey(name)) countStrings[name] = (countPrefix, countSuffix);
            return name;
        }

        internal static string GetFinalName(string name, int mentions) =>
            !countStrings.TryGetValue(name, out (string prefix, (string singular, string plural) suffix) strings) ? name
                : name + $" ({strings.prefix}{mentions}{(mentions == 1 ? strings.suffix.singular : strings.suffix.plural)})";

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
            if (defaultPolicyCache.TryGetValue(name, out policy))
                return !(policy is null);
            foreach (PropertyInfo property in typeof(DefaultPolicies).GetProperties(BindingFlags.Public | BindingFlags.Static))
                if (property.PropertyType == typeof(PolicyObject))
                {
                    PolicyObject policyObject = (PolicyObject)property.GetValue(null, null);
                    if (!(policyObject is null) && policyObject.Name.ToString() == name)
                    {
                        defaultPolicyCache[name] = policyObject;
                        policy = policyObject;
                        return true;
                    }
                }
            defaultPolicyCache[name] = null;
            return false;
        }
    }
}