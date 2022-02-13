﻿using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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
                    if (TryGetSettlementFromName(name, out Settlement settlement) && settlement.IsVillage) // denars, food
                        name = SetupSorting("Village tax", "from ", (" village", " villages"));
                    else if (!(settlement is null) && settlement.IsCastle) // denars
                        name = SetupSorting("Castle tax", "from ", (" castle", " castles"));
                    else if (name.Contains("'s tariff") || !(settlement is null) && settlement.IsTown)  // denars
                    {
                        name = SetupSorting("Town tax & tariffs", "from ", (" town", " towns"));
                        if (!(settlement is null) && settlement.IsTown) incrementMentions = false;
                    }
                    else if (TryGetKingdomPolicyFromName(name, out _)) // denars, influence
                        name = SetupSorting("Kingdom policies", "from ", (" policy", " policies"));
                    else if (name.Contains("Party wages Garrison of ")) // denars
                        name = SetupSorting("Garrison wages", "for ", (" garrison", " garrisons"));
                    else if (name.Contains("Party wages ")) // denars
                        name = SetupSorting("Party wages", "for ", (" party", " parties"));
                    else if (name.Contains("Caravan (")) // denars
                        name = SetupSorting("Caravan balance", "from ", (" caravan", " caravans"));
                    else if (name.Contains("Tribute from ")) // denars
                        name = SetupSorting("Tribute", "from ", (" kingdom", " kingdoms"));
                    else if (TryGetBuildingTypeFromName(name, out BuildingType buildingType) && buildingType.GetBaseBuildingEffectAmount(BuildingEffectEnum.FoodProduction, buildingType.StartLevel) > 0) // food
                        name = SetupSorting("Building production", "from ", (" building", " buildings"));
                    else if (TryGetItemCategoryFromName(name, out ItemCategory itemCategory) && itemCategory.Properties == ItemCategory.Property.BonusToFoodStores) // food
                        name = SetupSorting("Sold food goods", "from ", (" good", " goods"));
                    // Improved Garrisons support
                    else if (name.Contains("Improved Garrison Training of ")) // denars
                        name = SetupSorting("Garrison training", "for ", (" garrison", " garrisons"));
                    else if (name.Contains("Garrisonguards wages")) // denars
                        name = SetupSorting("Garrisonguard wages", "for ", (" garrisonguard", " garrisonguards"));
                    else if (name.Contains(" costs")) // denars
                        name = SetupSorting("Garrison recruitment", "for ", (" recruiter", " recruiters"));
                    else if (name.Contains("finance help")) // denars
                        name = SetupSorting("Garrison financial help", "for ", (" garrison", " garrisons"));
                    // Populations of Calradia support
                    else if (name.Contains("Excess noble population at ")) // influence
                        name = SetupSorting("Excess noble population", "at ", (" settlement", " settlements"));
                    else if (name.Contains("Nobles influence from ")) // influence
                        name = SetupSorting("Nobles influence", "from ", (" settlement", " settlements"));
                    else if (name.Contains("Population growth policy at ")) // influence
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

        private static MBReadOnlyList<PolicyObject> policies = null;
        private static readonly Dictionary<string, PolicyObject> policyObjectCache = new Dictionary<string, PolicyObject>();
        private static bool TryGetKingdomPolicyFromName(string name, out PolicyObject policyObject)
        {
            if (policyObjectCache.TryGetValue(name, out policyObject))
                return !(policyObject is null);
            if (policies is null) policies = (MBReadOnlyList<PolicyObject>)typeof(Campaign)
                    .GetProperty("AllPolicies", (BindingFlags)(-1)).GetMethod.Invoke(Campaign.Current, new object[0]);
            foreach (PolicyObject _policyObject in policies)
                if (_policyObject.Name.ToString() == name)
                {
                    policyObject = _policyObject;
                    policyObjectCache[name] = policyObject;
                    return true;
                }
            policyObjectCache[name] = null;
            return false;
        }

        private static MBReadOnlyList<BuildingType> buildingTypes = null;
        private static readonly Dictionary<string, BuildingType> buildingTypesCache = new Dictionary<string, BuildingType>();
        private static bool TryGetBuildingTypeFromName(string name, out BuildingType buildingType)
        {
            if (buildingTypesCache.TryGetValue(name, out buildingType))
                return !(buildingType is null);
            if (buildingTypes is null) buildingTypes = (MBReadOnlyList<BuildingType>)typeof(Campaign)
                    .GetProperty("AllBuildingTypes", (BindingFlags)(-1)).GetMethod.Invoke(Campaign.Current, new object[0]);
            foreach (BuildingType _buildingType in buildingTypes)
                if (_buildingType.Name.ToString() == name)
                {
                    buildingType = _buildingType;
                    buildingTypesCache[name] = buildingType;
                    return true;
                }
            buildingTypesCache[name] = null;
            return false;
        }

        private static MBReadOnlyList<ItemCategory> itemCategories = null;
        private static readonly Dictionary<string, ItemCategory> itemCategoryCache = new Dictionary<string, ItemCategory>();
        private static bool TryGetItemCategoryFromName(string name, out ItemCategory itemCategory)
        {
            if (itemCategoryCache.TryGetValue(name, out itemCategory))
                return !(itemCategory is null);
            if (itemCategories is null) itemCategories = (MBReadOnlyList<ItemCategory>)typeof(Campaign)
                    .GetProperty("AllItemCategories", (BindingFlags)(-1)).GetMethod.Invoke(Campaign.Current, new object[0]);
            foreach (ItemCategory _itemCategory in itemCategories)
                if (_itemCategory.GetName().ToString() == name)
                {
                    itemCategory = _itemCategory;
                    itemCategoryCache[name] = itemCategory;
                    return true;
                }
            itemCategoryCache[name] = null;
            return false;
        }
    }
}