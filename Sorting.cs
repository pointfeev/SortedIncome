using System;
using System.Collections.Generic;
using System.Linq;
using SortedIncome.Utilities;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.Core.ViewModelCollection.Information.TooltipProperty;

namespace SortedIncome
{
    internal static class Sorting
    {
        private static Func<List<TooltipProperty>> currentTooltipFunc;

        private static bool leftAltDown = InputKey.LeftAlt.IsDown();

        private static readonly Dictionary<string, (string prefix, (string singular, string plural) suffix)> Strings
            = new Dictionary<string, (string prefix, (string singular, string plural) suffix)>();

        private static readonly Dictionary<string, (float number, int mentions, int textHeight)> Lines
            = new Dictionary<string, (float number, int mentions, int textHeight)>();

        private static MBReadOnlyList<Settlement> settlements;
        private static readonly Dictionary<string, Settlement> SettlementCache = new Dictionary<string, Settlement>();

        private static MBReadOnlyList<PolicyObject> policyObjects;

        private static readonly Dictionary<string, PolicyObject> PolicyObjectCache
            = new Dictionary<string, PolicyObject>();

        private static MBReadOnlyList<BuildingType> buildingTypes;

        private static readonly Dictionary<string, BuildingType> BuildingTypesCache
            = new Dictionary<string, BuildingType>();

        private static MBReadOnlyList<ItemCategory> itemCategories;

        private static readonly Dictionary<string, ItemCategory> ItemCategoryCache
            = new Dictionary<string, ItemCategory>();

        internal static void BeginTooltip(Func<List<TooltipProperty>> ____tooltipProperties)
            => currentTooltipFunc = ____tooltipProperties;

        internal static void ShowTooltip(Type type)
        {
            if (type != typeof(List<TooltipProperty>))
                currentTooltipFunc = null;
        }

        internal static void TickTooltip(PropertyBasedTooltipVM __instance)
        {
            if (!__instance.IsActive) return;
            bool leftAltDown = InputKey.LeftAlt.IsDown();
            if (Sorting.leftAltDown != leftAltDown && !(currentTooltipFunc is null))
                InformationManager.ShowTooltip(typeof(List<TooltipProperty>), currentTooltipFunc());
            Sorting.leftAltDown = leftAltDown;
        }

        internal static void GetTooltip(ref List<TooltipProperty> __result)
        {
            if (InputKey.LeftAlt.IsDown()) return;
            SortTooltip(__result);
        }

        private static void SortTooltip(IList<TooltipProperty> properties)
        {
            try
            {
                Strings.Clear();
                Lines.Clear();
                int start = -1, end = -1;
                for (int i = 0; i < properties.Count; i++)
                {
                    TooltipProperty property = properties[i];
                    if (property.PropertyModifier == (int)TooltipPropertyFlags.None)
                    {
                        if (start == -1) start = i;
                        bool incrementMentions = true;
                        string name = property.DefinitionLabel;
                        if (TryGetSettlementFromName(name, out Settlement settlement)
                         && settlement.IsVillage) // denars, food
                        {
                            name = GetStrings("Village tax", "from", ("village", "villages"));
                        }
                        else if (!(settlement is null) && settlement.IsCastle) // denars
                        {
                            name = GetStrings("Castle tax", "from", ("castle", "castles"));
                        }
                        else if (name.Parse("'s tariff", "{wVMPdc8J}")
                              || (!(settlement is null) && settlement.IsTown)) // denars
                        {
                            if (settlement is null || !settlement.IsTown) incrementMentions = false;
                            name = GetStrings("Town tax & tariffs", "from", ("town", "towns"));
                        }
                        else if
                            (TryGetKingdomPolicyFromName(
                                name, out _)) // denars, militia, food, loyalty, security, prosperity, settlement tax
                        {
                            name = GetStrings("Kingdom policies", "from", ("policy", "policies"));
                        }
                        else if (TryGetBuildingTypeFromName(name, out BuildingType buildingType)
                              && buildingType.GetBaseBuildingEffectAmount(
                                     BuildingEffectEnum.FoodProduction, buildingType.StartLevel) > 0) // food
                        {
                            name = GetStrings("Building production", "from", ("building", "buildings"));
                        }
                        else if (TryGetItemCategoryFromName(name, out ItemCategory itemCategory)
                              && itemCategory.Properties == ItemCategory.Property.BonusToFoodStores) // food
                        {
                            name = GetStrings("Sold food goods", "from", ("good", "goods"));
                        }
                        else if (name.Parse("Party expenses ", "{dZDFxUvU}")
                              && name.Parse("Garrison of ", "{frt7AmX0}")) // denars
                        {
                            name = GetStrings("Garrison expenses", "for", ("garrison", "garrisons"));
                        }
                        else if (!name.Parse("Main party wages", "{YkZKXsIn}")
                              && name.Parse("Party expenses ", "{dZDFxUvU}")) // denars
                        {
                            name = GetStrings("Party expenses", "for", ("party", "parties"));
                        }
                        else if (name.Parse("Caravan (", "{c2pdihCB}")) // denars
                        {
                            name = GetStrings("Caravan balance", "from", ("caravan", "caravans"));
                        }
                        else if (name.Parse("Tribute from ", "{rhfgzKtA}")) // denars
                        {
                            name = GetStrings("Tribute", "from", ("kingdom", "kingdoms"));
                        }
                        // Improved Garrisons support
                        else if (name.Parse("Improved Garrison Training of ",
                                            "{misc_costmodel_trainingcosts}")) // denars
                        {
                            name = GetStrings("Garrison training", "for", ("garrison", "garrisons"));
                        }
                        else if (name.Parse("Improved Garrison Recruitment of ",
                                            "{misc_costmodel_recruitmentcosts}")) // denars
                        {
                            name = GetStrings("Garrison recruitment", "for", ("garrison", "garrisons"));
                        }
                        else if (name.Parse(" Guard wages", "{misc_guardwages}")) // denars
                        {
                            name = GetStrings("Garrison guard wages", "for", ("garrison guard", "garrison guards"));
                        }
                        else if (name.Parse(" finance help", "{rhKxsdtz}")) // denars
                        {
                            name = GetStrings("Garrison financial help", "for", ("garrison", "garrisons"));
                        }
                        // Populations of Calradia and Banner Kings support
                        else if (name.Parse("Excess noble population at ")) // influence
                        {
                            name = GetStrings("Excess noble population", "at", ("settlement", "settlements"));
                        }
                        else if (name.Parse("Nobles influence from ")) // influence
                        {
                            name = GetStrings("Nobles influence", "from", ("settlement", "settlements"));
                        }
                        else if (name.Parse("Population growth policy at ")) // influence
                        {
                            name = GetStrings("Population growth policies", "at", ("settlement", "settlements"));
                        }
                        string value = property.ValueLabel;
                        if (!float.TryParse(value, out float number))
                            number = float.NaN;
                        int increment = incrementMentions ? 1 : 0;
                        int textHeight = property.TextHeight;
                        Lines[name] = Lines.TryGetValue(name, out (float number, int mentions, int textHeight) line)
                            ? (line.number + number, line.mentions + increment, Math.Max(line.textHeight, textHeight))
                            : (number, increment, textHeight);
                    }
                    else if (start != -1 && end == -1)
                    {
                        end = i - 1;
                    }
                }
                if (start == -1 || end == -1) return;
                for (int i = end; i >= start; i--)
                    properties.RemoveAt(i);
                foreach (KeyValuePair<string, (float number, int mentions, int textHeight)> line in Lines)
                {
                    string value = line.Value.number.ToString("0.##");
                    if (line.Value.number > 0.001f)
                    {
                        MBTextManager.SetTextVariable("NUMBER", value);
                        value = GameTexts.FindText("str_plus_with_number").ToString();
                    }
                    properties.Insert(start++,
                                      new TooltipProperty(GetFinalName(line.Key, line.Value.mentions), value,
                                                          line.Value.textHeight));
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        private static string GetStrings(string name, string countPrefix, (string singular, string plural) countSuffix)
        {
            name = name.TranslateWithDynamicId();
            countPrefix = countPrefix.TranslateWithDynamicId();
            countSuffix.singular = countSuffix.singular.TranslateWithDynamicId();
            countSuffix.plural = countSuffix.plural.TranslateWithDynamicId();
            if (!Strings.ContainsKey(name)) Strings[name] = (countPrefix, countSuffix);
            return name;
        }

        private static string GetFinalName(string name, int mentions) =>
            !Strings.TryGetValue(name, out (string prefix, (string singular, string plural) suffix) strings)
                ? name
                : name
                + $" ({strings.prefix} {mentions} {(mentions == 1 ? strings.suffix.singular : strings.suffix.plural)})";

        private static bool TryGetSettlementFromName(string name, out Settlement settlement)
        {
            if (SettlementCache.TryGetValue(name, out settlement))
                return !(settlement is null);
            if (Campaign.Current is null || (settlements = Settlement.All) is null)
                return false;
            foreach (Settlement _settlement in settlements.Where(_settlement => _settlement?.Name?.ToString() == name))
            {
                settlement = _settlement;
                SettlementCache[name] = settlement;
                return true;
            }
            return false;
        }

        private static bool TryGetKingdomPolicyFromName(string name, out PolicyObject policyObject)
        {
            if (PolicyObjectCache.TryGetValue(name, out policyObject))
                return !(policyObject is null);
            if (Campaign.Current is null || (policyObjects = PolicyObject.All) is null)
                return false;
            foreach (PolicyObject _policyObject in policyObjects.Where(_policyObject
                                                                           => _policyObject?.Name?.ToString() == name))
            {
                policyObject = _policyObject;
                PolicyObjectCache[name] = policyObject;
                return true;
            }
            return false;
        }

        private static bool TryGetBuildingTypeFromName(string name, out BuildingType buildingType)
        {
            if (BuildingTypesCache.TryGetValue(name, out buildingType))
                return !(buildingType is null);
            if (Campaign.Current is null || (buildingTypes = BuildingType.All) is null)
                return false;
            foreach (BuildingType _buildingType in buildingTypes.Where(_buildingType
                                                                           => _buildingType?.Name?.ToString() == name))
            {
                buildingType = _buildingType;
                BuildingTypesCache[name] = buildingType;
                return true;
            }
            return false;
        }

        private static bool TryGetItemCategoryFromName(string name, out ItemCategory itemCategory)
        {
            if (ItemCategoryCache.TryGetValue(name, out itemCategory))
                return !(itemCategory is null);
            if (Campaign.Current is null || (itemCategories = ItemCategories.All) is null)
                return false;
            foreach (ItemCategory _itemCategory in itemCategories.Where(
                         _itemCategory => _itemCategory?.GetName()?.ToString() == name))
            {
                itemCategory = _itemCategory;
                ItemCategoryCache[name] = itemCategory;
                return true;
            }
            return false;
        }
    }
}