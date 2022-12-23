﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static readonly Dictionary<string, (string prefix, (string singular, string plural) suffix)> Strings
            = new Dictionary<string, (string prefix, (string singular, string plural) suffix)>();

        private static readonly
            Dictionary<string, (float number, Dictionary<string, int> variationMentions, int textHeight)> Lines
                = new Dictionary<string, (float number, Dictionary<string, int> variationMentions, int textHeight)>();

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

        internal static MethodInfo AddLine;
        internal static FieldInfo Value;
        internal static readonly Dictionary<string, string> TextObjectStrs = new Dictionary<string, string>();

        private static Func<List<TooltipProperty>> currentTooltipFunc;

        private static object explainer;
        private static TextObject descriptionObj;
        private static TextObject variableObj;

        private static bool wasLeftAltDown = LeftAltDown;

        private static bool CanSort => AddLine != null && Value != null;
        private static bool LeftAltDown => InputKey.LeftAlt.IsDown();

        internal static bool ExplainedNumberPrefix(object ____explainer, ref TextObject description,
                                                   TextObject variable)
        {
            if (____explainer == null || description == null || variable == null || !CanSort || LeftAltDown)
                return true;
            explainer = ____explainer;
            descriptionObj = description;
            variableObj = variable;
            description = null;
            return true;
        }

        internal static void ExplainedNumberPostfix(float value)
        {
            if (explainer == null || descriptionObj == null || variableObj == null || !CanSort || LeftAltDown)
                return;
            string description = (string)Value.GetValue(descriptionObj);
            string variableValue = (string)Value.GetValue(variableObj);
            if (variableObj != null)
                description += ";;;" + variableValue
                                     + (variableObj.ToString() != variableValue ? ":::" + variableObj : "");
            _ = AddLine.Invoke(explainer, new object[] { description, value, 1 });
            descriptionObj = null;
            variableObj = null;
        }

        internal static void BeginTooltip(Func<List<TooltipProperty>> ____tooltipProperties)
        {
            if (!CanSort) return;
            currentTooltipFunc = ____tooltipProperties;
        }

        internal static void ShowTooltip(Type type)
        {
            if (!CanSort) return;
            if (type != typeof(List<TooltipProperty>))
                currentTooltipFunc = null;
        }

        internal static void TickTooltip(PropertyBasedTooltipVM __instance)
        {
            if (!__instance.IsActive || !CanSort) return;
            bool leftAltDown = LeftAltDown;
            if (wasLeftAltDown != leftAltDown && !(currentTooltipFunc is null))
                InformationManager.ShowTooltip(typeof(List<TooltipProperty>), currentTooltipFunc());
            wasLeftAltDown = leftAltDown;
        }

        internal static void GetTooltip(ref List<TooltipProperty> __result)
        {
            if (!CanSort || LeftAltDown) return;
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
                        string description = property.DefinitionLabel;
                        //string debug = description;
                        if (description.Length < 1)
                            continue;
                        string variableValue = null, variable = null;
                        int varValStart = description.IndexOf(";;;", StringComparison.Ordinal);
                        if (varValStart != -1)
                        {
                            variableValue = description.Substring(varValStart + 3);
                            description = description.Substring(0, varValStart);
                            if (description.Length < 1)
                                continue;
                            int varStart = variableValue.IndexOf(":::", StringComparison.Ordinal);
                            if (varStart != -1)
                            {
                                variable = variableValue.Substring(varStart + 3);
                                variableValue = variableValue.Substring(0, varStart);
                            }
                        }
                        string variation = description;
                        if (description == TextObjectStrs["partyIncome"]
                         || description == TextObjectStrs["partyExpenses"])
                        {
                            // denars
                            description = variableValue
                                       == (string)Value.GetValue(GameTexts.FindText("str_garrison_party_name"))
                                ? SetupStrings("Garrison expenses", "for", ("garrison", "garrisons"))
                                : SetupStrings("Party balance", "from", ("party", "parties"));
                        }
                        else if (description == TextObjectStrs["caravanIncome"])
                        {
                            // denars
                            description = SetupStrings("Caravan balance", "from", ("caravan", "caravans"));
                        }
                        else if (description == TextObjectStrs["tributeIncome"])
                        {
                            // denars
                            description = SetupStrings("Tribute", "from", ("kingdom", "kingdoms"));
                        }
                        else if ((TryGetSettlementFromName(description, out Settlement settlement)
                               && settlement.IsVillage)
                              || description == TextObjectStrs["villageIncome"])
                        {
                            // denars, food
                            description = SetupStrings("Village tax", "from", ("village", "villages"));
                        }
                        else if ((settlement != null && settlement.IsTown)
                              || description == TextObjectStrs["townTax"]
                              || description == TextObjectStrs["townTradeTax"]
                              || description == TextObjectStrs["tariffTax"])
                        {
                            // denars
                            description = SetupStrings("Town tax & tariffs", "from", ("town", "towns"));
                        }
                        else if (settlement != null && settlement.IsCastle)
                        {
                            // denars
                            description = SetupStrings("Castle tax", "from", ("castle", "castles"));
                        }
                        else if (TryGetKingdomPolicyFromName(description, out _))
                        {
                            // denars, militia, food, loyalty, security, prosperity, settlement tax
                            description = SetupStrings("Kingdom policies", "from", ("policy", "policies"));
                        }
                        else if (TryGetBuildingTypeFromName(description, out BuildingType buildingType)
                              && buildingType.GetBaseBuildingEffectAmount(
                                     BuildingEffectEnum.FoodProduction, buildingType.StartLevel) > 0)
                        {
                            // food
                            description = SetupStrings("Building production", "from", ("building", "buildings"));
                        }
                        else if (TryGetItemCategoryFromName(description, out ItemCategory itemCategory)
                              && itemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
                        {
                            // food
                            description = SetupStrings("Sold food goods", "from", ("good", "goods"));
                        }
                        // Improved Garrisons support
                        else if (description.StartsWith("{=misc_costmodel_trainingcosts}")
                              || description.StartsWith("Improved Garrison Training of "))
                        {
                            // denars
                            description = SetupStrings("Garrison training", "for", ("garrison", "garrisons"));
                        }
                        else if (description.StartsWith("{=misc_costmodel_recruitmentcosts}")
                              || description.StartsWith("Improved Garrison Recruitment of "))
                        {
                            // denars
                            description = SetupStrings("Garrison recruitment", "for", ("garrison", "garrisons"));
                        }
                        else if (description.StartsWith("{=misc_guardwages}") || description.EndsWith(" Guard wages"))
                        {
                            // denars
                            description = SetupStrings("Garrison guard wages", "for",
                                                       ("garrison guard", "garrison guards"));
                        }
                        else if (description.StartsWith("{=rhKxsdtz}") || description.EndsWith(" finance help"))
                        {
                            // denars
                            description = SetupStrings("Garrison financial help", "for", ("garrison", "garrisons"));
                        }
                        else
                        {
                            TextObject nameObj = new TextObject(description);
                            if (variable != null || variableValue != null)
                                nameObj.SetTextVariable("A0", variable ?? variableValue);
                            //description = debug + "===" + nameObj;
                            description = nameObj.ToString();
                        }
                        string value = property.ValueLabel;
                        if (!float.TryParse(value, out float number))
                            number = float.NaN;
                        int textHeight = property.TextHeight;
                        if (Lines.TryGetValue(description,
                                              out (float number, Dictionary<string, int> variationMentions, int
                                              textHeight) line))
                        {
                            line.variationMentions.TryGetValue(variation, out int mentions);
                            line.variationMentions[variation] = mentions + 1;
                            Lines[description] = (
                                line.number + number, line.variationMentions, Math.Max(line.textHeight, textHeight));
                        }
                        else
                        {
                            Lines[description] = (number, new Dictionary<string, int> { [variation] = 1 }, textHeight);
                        }
                    }
                    else if (start != -1 && end == -1)
                    {
                        end = i - 1;
                    }
                }
                if (start == -1 || end == -1) return;
                for (int i = end; i >= start; i--)
                    properties.RemoveAt(i);
                foreach (KeyValuePair<string, (float number, Dictionary<string, int> variationMentions, int textHeight)>
                             line in Lines)
                {
                    string value = line.Value.number.ToString("0.##");
                    if (line.Value.number > 0.001f)
                    {
                        MBTextManager.SetTextVariable("NUMBER", value);
                        value = GameTexts.FindText("str_plus_with_number").ToString();
                    }
                    properties.Insert(start++,
                                      new TooltipProperty(
                                          GetFinalDescription(line.Key, line.Value.variationMentions.Max(v => v.Value)),
                                          value,
                                          line.Value.textHeight));
                }
            }
            catch (Exception e)
            {
                OutputUtils.DoOutputForException(e);
            }
        }

        private static string SetupStrings(string name, string countPrefix,
                                           (string singular, string plural) countSuffix)
        {
            name = name.TranslateWithDynamicId();
            countPrefix = countPrefix.TranslateWithDynamicId();
            countSuffix.singular = countSuffix.singular.TranslateWithDynamicId();
            countSuffix.plural = countSuffix.plural.TranslateWithDynamicId();
            if (!Strings.ContainsKey(name)) Strings[name] = (countPrefix, countSuffix);
            return name;
        }

        private static string GetFinalDescription(string name, int mentions) =>
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
            foreach (Settlement s in settlements.Where(s => s?.Name?.ToString() == name))
            {
                settlement = s;
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
            foreach (PolicyObject p in policyObjects.Where(p => p?.Name?.ToString() == name))
            {
                policyObject = p;
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
            foreach (BuildingType t in buildingTypes.Where(t => t?.Name?.ToString() == name))
            {
                buildingType = t;
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
            foreach (ItemCategory c in itemCategories.Where(c => c?.GetName()?.ToString() == name))
            {
                itemCategory = c;
                ItemCategoryCache[name] = itemCategory;
                return true;
            }
            return false;
        }
    }
}