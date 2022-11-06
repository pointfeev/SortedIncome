using System;
using System.Collections.Generic;
using System.Reflection;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
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
                        name = SetupSorting("Village tax", "from", ("village", "villages"));
                    else if (!(settlement is null) && settlement.IsCastle) // denars
                        name = SetupSorting("Castle tax", "from", ("castle", "castles"));
                    else if (name.Parse("'s tariff", "{wVMPdc8J}") || !(settlement is null) && settlement.IsTown)  // denars
                    {
                        if (settlement is null || !settlement.IsTown) incrementMentions = false;
                        name = SetupSorting("Town tax & tariffs", "from", ("town", "towns"));
                    }
                    else if (TryGetKingdomPolicyFromName(name, out _)) // denars, militia, food, loyalty, security, prosperity, settlement tax
                        name = SetupSorting("Kingdom policies", "from", ("policy", "policies"));
                    else if (TryGetBuildingTypeFromName(name, out BuildingType buildingType) && buildingType.GetBaseBuildingEffectAmount(BuildingEffectEnum.FoodProduction, buildingType.StartLevel) > 0) // food
                        name = SetupSorting("Building production", "from", ("building", "buildings"));
                    else if (TryGetItemCategoryFromName(name, out ItemCategory itemCategory) && itemCategory.Properties == ItemCategory.Property.BonusToFoodStores) // food
                        name = SetupSorting("Sold food goods", "from", ("good", "goods"));
                    else if (name.Parse("Party expenses ", "{dZDFxUvU}") && name.Parse("Garrison of ", "{frt7AmX0}")) // denars
                        name = SetupSorting("Garrison expenses", "for", ("garrison", "garrisons"));
                    else if (!name.Parse("Main party wages", "{YkZKXsIn}") && name.Parse("Party expenses ", "{dZDFxUvU}")) // denars
                        name = SetupSorting("Party expenses", "for", ("party", "parties"));
                    else if (name.Parse("Caravan (", "{c2pdihCB}")) // denars
                        name = SetupSorting("Caravan balance", "from", ("caravan", "caravans"));
                    else if (name.Parse("Tribute from ", "{rhfgzKtA}")) // denars
                        name = SetupSorting("Tribute", "from", ("kingdom", "kingdoms"));
                    // Improved Garrisons support
                    else if (name.Parse("Improved Garrison Training of ", "{misc_costmodel_trainingcosts}")) // denars
                        name = SetupSorting("Garrison training", "for", ("garrison", "garrisons"));
                    else if (name.Parse("Improved Garrison Recruitment of ", "{misc_costmodel_recruitmentcosts}")) // denars
                        name = SetupSorting("Garrison recruitment", "for", ("garrison", "garrisons"));
                    else if (name.Parse(" Guard wages", "{misc_guardwages}")) // denars
                        name = SetupSorting("Garrison guard wages", "for", ("garrison guard", "garrison guards"));
                    else if (name.Parse(" finance help", "{rhKxsdtz}")) // denars
                        name = SetupSorting("Garrison financial help", "for", ("garrison", "garrisons"));
                    // Populations of Calradia and Banner Kings support
                    else if (name.Parse("Excess noble population at ")) // influence
                        name = SetupSorting("Excess noble population", "at", ("settlement", "settlements"));
                    else if (name.Parse("Nobles influence from ")) // influence
                        name = SetupSorting("Nobles influence", "from", ("settlement", "settlements"));
                    else if (name.Parse("Population growth policy at ")) // influence
                        name = SetupSorting("Population growth policies", "at", ("settlement", "settlements"));
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
            name = name.TranslateWithDynamicID();
            countPrefix = countPrefix.TranslateWithDynamicID();
            countSuffix.singular = countSuffix.singular.TranslateWithDynamicID();
            countSuffix.plural = countSuffix.plural.TranslateWithDynamicID();
            if (!countStrings.ContainsKey(name)) countStrings[name] = (countPrefix, countSuffix);
            return name;
        }

        internal static string GetFinalName(string name, int mentions) =>
            !countStrings.TryGetValue(name, out (string prefix, (string singular, string plural) suffix) strings) ? name
                : name + $" ({strings.prefix} {mentions} {(mentions == 1 ? strings.suffix.singular : strings.suffix.plural)})";

        private static MBReadOnlyList<Settlement> settlements;
        private static readonly Dictionary<string, Settlement> settlementCache = new Dictionary<string, Settlement>();
        private static bool TryGetSettlementFromName(string name, out Settlement settlement)
        {
            if (settlementCache.TryGetValue(name, out settlement))
                return !(settlement is null);
            if (!(Campaign.Current is null) && !((settlements = Settlement.All) is null))
                foreach (Settlement _settlement in settlements)
                    if (_settlement?.Name?.ToString() == name)
                    {
                        settlement = _settlement;
                        settlementCache[name] = settlement;
                        return true;
                    }
            return false;
        }

        private static MBReadOnlyList<PolicyObject> policyObjects;
        private static readonly Dictionary<string, PolicyObject> policyObjectCache = new Dictionary<string, PolicyObject>();
        private static bool TryGetKingdomPolicyFromName(string name, out PolicyObject policyObject)
        {
            if (policyObjectCache.TryGetValue(name, out policyObject))
                return !(policyObject is null);
            if (!(Campaign.Current is null) && !((policyObjects = PolicyObject.All) is null))
                foreach (PolicyObject _policyObject in policyObjects)
                    if (_policyObject?.Name?.ToString() == name)
                    {
                        policyObject = _policyObject;
                        policyObjectCache[name] = policyObject;
                        return true;
                    }
            return false;
        }

        private static MBReadOnlyList<BuildingType> buildingTypes;
        private static readonly Dictionary<string, BuildingType> buildingTypesCache = new Dictionary<string, BuildingType>();
        private static bool TryGetBuildingTypeFromName(string name, out BuildingType buildingType)
        {
            if (buildingTypesCache.TryGetValue(name, out buildingType))
                return !(buildingType is null);
            if (!(Campaign.Current is null) && !((buildingTypes = BuildingType.All) is null))
                foreach (BuildingType _buildingType in buildingTypes)
                    if (_buildingType?.Name?.ToString() == name)
                    {
                        buildingType = _buildingType;
                        buildingTypesCache[name] = buildingType;
                        return true;
                    }
            return false;
        }

        private static MBReadOnlyList<ItemCategory> itemCategories;
        private static readonly Dictionary<string, ItemCategory> itemCategoryCache = new Dictionary<string, ItemCategory>();
        private static bool TryGetItemCategoryFromName(string name, out ItemCategory itemCategory)
        {
            if (itemCategoryCache.TryGetValue(name, out itemCategory))
                return !(itemCategory is null);
            if (!(Campaign.Current is null) && !((itemCategories = ItemCategories.All) is null))
                foreach (ItemCategory _itemCategory in itemCategories)
                    if (_itemCategory?.GetName()?.ToString() == name)
                    {
                        itemCategory = _itemCategory;
                        itemCategoryCache[name] = itemCategory;
                        return true;
                    }
            return false;
        }
    }
}