using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using SortedIncome.Utilities;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace SortedIncome;

internal static class Sorting
{
    private static readonly Dictionary<string, (string prefix, (string singular, string plural) suffix)> Strings = new();

    private static readonly Dictionary<string, (float number, Dictionary<object, int> variationMentions, int textHeight)> Lines = new();

    private static MBReadOnlyList<Settlement> settlements;
    private static readonly Dictionary<string, Settlement> SettlementCache = new();

    private static MBReadOnlyList<PolicyObject> policyObjects;
    private static readonly Dictionary<string, PolicyObject> PolicyObjectCache = new();

    private static MBReadOnlyList<BuildingType> buildingTypes;
    private static readonly Dictionary<string, BuildingType> BuildingTypesCache = new();

    private static MBReadOnlyList<ItemCategory> itemCategories;
    private static readonly Dictionary<string, ItemCategory> ItemCategoryCache = new();

    private static MBReadOnlyList<PerkObject> perkObjects;
    private static readonly Dictionary<string, PerkObject> PerkObjectCache = new();

    internal static FieldInfo Value;
    internal static readonly Dictionary<string, string> ModelTextValues = new();
    internal static MethodInfo AddLine;
    internal static object OperationType;

    private static Func<List<TooltipProperty>> currentTooltipFunc;

    private static bool wasLeftAltDown = LeftAltDown;
    private static bool LeftAltDown => InputKey.LeftAlt.IsDown();

    internal static void IncludeDetails(bool applyWithdrawals, ref bool includeDetails) => includeDetails = !applyWithdrawals;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetModelTextValue(string key, bool ignoreFailure = false)
    {
        if (ModelTextValues.TryGetValue(key, out string str))
            return str;
        if (!ignoreFailure)
            OutputUtils.DoOutput(new("Failed to get DefaultClanFinanceModel TextObject field: " + key));
        return string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetModelTextValue(string key, out string str, bool ignoreFailure = false)
    {
        str = GetModelTextValue(key, ignoreFailure);
        return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetGameTextValue(string key, bool ignoreFailure = false)
    {
        string str = (string)Value.GetValue(GameTexts.FindText(key));
        if (!str.StartsWith("{=!}ERROR"))
            return str;
        if (!ignoreFailure)
            OutputUtils.DoOutput(new("Failed to get GameTexts TextObject field: " + key));
        return string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryConvertExplainedNumber(TextObject description, TextObject variable, out string conversion)
    {
        try
        {
            string descriptionValue = Value.GetValue(description) as string;
            if (descriptionValue is not null && Value.GetValue(variable) is string variableValue)
                descriptionValue += ";;;" + variableValue + (variable.ToString() != variableValue ? ":::" + variable : "");
            if (descriptionValue is null || descriptionValue.Length == 0)
                throw new ArgumentException();
            conversion = descriptionValue;
            return true;
        }
        catch
        {
            conversion = null;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryUnconvertExplainedNumber(ref string description, out string variableValue, out string variable)
    {
        try
        {
            if (description.Length == 0)
                throw new ArgumentException();
            int varValStart = description.IndexOf(";;;", StringComparison.Ordinal);
            if (varValStart != -1)
            {
                variableValue = description.Substring(varValStart + 3);
                description = description.Substring(0, varValStart);
                if (description.Length == 0)
                    throw new ArgumentException();
                int varStart = variableValue.IndexOf(":::", StringComparison.Ordinal);
                if (varStart != -1)
                {
                    variable = variableValue.Substring(varStart + 3);
                    variableValue = variableValue.Substring(0, varStart);
                }
                else
                    variable = null;
            }
            else
            {
                variableValue = null;
                variable = null;
            }
            return true;
        }
        catch
        {
            variableValue = null;
            variable = null;
            return false;
        }
    }

    internal static bool AddTooltip(object ____explainer, float value, ref TextObject description, TextObject variable)
    {
        if (____explainer == null || description == null || variable == null || value.ApproximatelyEqualsTo(0f) || LeftAltDown)
            return true;
        if (!TryConvertExplainedNumber(description, variable, out string conversion))
            return true;
        try
        {
            _ = AddLine.Invoke(____explainer, new[] { conversion ?? description.ToString(), value, OperationType });
        }
        catch
        {
            return true;
        }
        description = null; // prevent original AddLine from being called
        return true;
    }

    internal static Exception BeginTooltip(Exception __exception, Func<List<TooltipProperty>> ____tooltipProperties)
    {
        currentTooltipFunc = ____tooltipProperties;
        /*if (__exception != null)
            OutputUtils.DoOutputForFinalizer(__exception);*/
        return null;
    }

    internal static Exception ShowTooltip(Exception __exception, Type type)
    {
        if (type != typeof(List<TooltipProperty>))
            currentTooltipFunc = null;
        /*if (__exception != null)
            OutputUtils.DoOutputForFinalizer(__exception);*/
        return null;
    }

    private static void ResetTooltip(PropertyBasedTooltipVM __instance)
    {
        if (currentTooltipFunc == null || __instance is not { IsActive: true })
            return;
        try
        {
            InformationManager.ShowTooltip(typeof(List<TooltipProperty>), currentTooltipFunc());
        }
        catch
        {
            // ignore
        }
    }

    internal static Exception TickTooltip(Exception __exception, PropertyBasedTooltipVM __instance)
    {
        bool leftAltDown = LeftAltDown;
        if (wasLeftAltDown != leftAltDown)
        {
            wasLeftAltDown = leftAltDown;
            ResetTooltip(__instance);
        }
        /*if (__exception != null)
            OutputUtils.DoOutputForFinalizer(__exception);*/
        return null;
    }

    internal static Exception GetTooltip(Exception __exception, ref List<TooltipProperty> __result)
    {
        if (!LeftAltDown)
            SortTooltip(ref __result);
        /*if (__exception != null)
            OutputUtils.DoOutputForFinalizerDoOutputForFinalizer(__exception);*/
        return null;
    }

    private static void SortTooltip(ref List<TooltipProperty> properties)
    {
        try
        {
            Strings.Clear();
            Lines.Clear();
            int start = -1, end = -1;
            for (int i = 0; i < properties.Count; i++)
            {
                TooltipProperty property = properties[i];
                if (property.PropertyModifier == (int)TooltipProperty.TooltipPropertyFlags.None)
                {
                    if (start == -1)
                        start = i;
                    string description = property.DefinitionLabel;
                    //string debug = description;
                    if (!TryUnconvertExplainedNumber(ref description, out string variableValue, out string variable))
                        continue;
                    object variation = false;
                    if (description == GetModelTextValue("caravanIncome", out string caravanIncome) || description == GetModelTextValue("partyIncome")
                                                                                                    || description == GetModelTextValue(
                                                                                                           "partyExpenses")) // denars
                        description = description == caravanIncome
                            ? SetupStrings("Caravan balance", "from", ("caravan", "caravans"))
                            : variableValue != null && (variableValue.StartsWith("{=fsTBcLvA}")
                                                     || variableValue == GetGameTextValue("str_garrison_party_name")) // for pre-v1.1.0
                                ? SetupStrings("Garrison expenses", "for", ("garrison", "garrisons"))
                                : SetupStrings("Party balance", "from", ("party", "parties"));
                    else if (description == GetModelTextValue("tributeIncome")) // denars
                        description = SetupStrings("Tribute", "from", ("kingdom", "kingdoms"));
                    else if (description == GetModelTextValue("townTax", out string townTax)
                           | description == GetModelTextValue("tariffTax", out string tariffTax)
                           | (TryGetSettlementFromName(description, out Settlement settlement)
                           || TryGetSettlementFromName(variable, out settlement))) // denars, food
                    {
                        if (description == townTax || description == tariffTax)
                            variation = description;
                        description = settlement switch
                        {
                            { IsVillage: true } => SetupStrings("Village tax", "from", ("village", "villages")),
                            { IsCastle: true } => SetupStrings("Castle tax", "from", ("castle", "castles")),
                            _ => SetupStrings("Town tax & tariffs", "from", ("town", "towns"))
                        };
                    }
                    else if (TryGetKingdomPolicyFromName(description, out _)) // denars, militia, food, loyalty, security, prosperity, settlement tax
                        description = SetupStrings("Kingdom policies", "from", ("policy", "policies"));
                    else if (TryGetBuildingTypeFromName(description, out BuildingType buildingType)
                          && buildingType.GetBaseBuildingEffectAmount(BuildingEffectEnum.FoodProduction, buildingType.StartLevel) > 0) // food
                        description = SetupStrings("Building production", "from", ("building", "buildings"));
                    else if (TryGetItemCategoryFromName(description, out ItemCategory itemCategory)
                          && itemCategory.Properties is ItemCategory.Property.BonusToFoodStores) // food
                        description = SetupStrings("Sold food goods", "from", ("good", "goods"));
                    /*else if (TryGetPerkObjectFromName(description, out PerkObject perkObject) && (perkObject.PrimaryRole is SkillEffect.PerkRole.Governor
                                                                                               || perkObject.SecondaryRole is SkillEffect.PerkRole.Governor))
                        description = SetupStrings("Governor perks", "from", ("perk", "perks")); // militia, food, loyalty, security, prosperity, settlement tax*/
                    // Improved Garrisons support
                    else if (description.StartsWith("{=misc_costmodel_trainingcosts}") || description.StartsWith("Improved Garrison Training of ")) // denars
                        description = SetupStrings("Garrison training", "for", ("garrison", "garrisons"));
                    else if (description.StartsWith("{=misc_costmodel_recruitmentcosts}")
                          || description.StartsWith("Improved Garrison Recruitment of ")) // denars
                        description = SetupStrings("Garrison recruitment", "for", ("garrison", "garrisons"));
                    else if (description.StartsWith("{=misc_guardwages}") || description.EndsWith(" Guard wages")) // denars
                        description = SetupStrings("Garrison guard wages", "for", ("garrison guard", "garrison guards"));
                    else if (description.StartsWith("{=rhKxsdtz}") || description.EndsWith(" finance help")) // denars
                        description = SetupStrings("Garrison financial help", "for", ("garrison", "garrisons"));
                    else
                    {
                        TextObject nameObj = new(description);
                        if (variable != null || variableValue != null)
                            _ = nameObj.SetTextVariable("A0", variable ?? variableValue);
                        //description = debug + "===" + nameObj;
                        description = nameObj.ToString();
                    }
                    string value = property.ValueLabel;
                    if (!float.TryParse(value, out float number))
                        number = float.NaN;
                    int textHeight = property.TextHeight;
                    if (Lines.TryGetValue(description, out (float number, Dictionary<object, int> variationMentions, int textHeight) line))
                    {
                        _ = line.variationMentions.TryGetValue(variation, out int mentions);
                        line.variationMentions[variation] = mentions + 1;
                        Lines[description] = (line.number + number, line.variationMentions, Math.Max(line.textHeight, textHeight));
                    }
                    else
                        Lines[description] = (number, new() { [variation] = 1 }, textHeight);
                }
                else if (start != -1 && end == -1)
                    end = i - 1;
            }
            if (start == -1 || end == -1)
                return;
            for (int i = end; i >= start; i--)
                properties.RemoveAt(i);
            foreach (KeyValuePair<string, (float number, Dictionary<object, int> variationMentions, int textHeight)> line in Lines)
            {
                string value = line.Value.number.ToString("0.##");
                if (line.Value.number > 0.001f)
                {
                    MBTextManager.SetTextVariable("NUMBER", value);
                    value = GameTexts.FindText("str_plus_with_number").ToString();
                }
                properties.Insert(start++, new(GetFinalDescription(line.Key, line.Value.variationMentions.Max(v => v.Value)), value, line.Value.textHeight));
            }
        }
        catch (Exception e)
        {
            OutputUtils.DoOutputForException(e);
        }
    }

    private static string SetupStrings(string name, string countPrefix, (string singular, string plural) countSuffix)
    {
        name = name.TranslateWithDynamicId();
        countPrefix = countPrefix.TranslateWithDynamicId();
        countSuffix.singular = countSuffix.singular.TranslateWithDynamicId();
        countSuffix.plural = countSuffix.plural.TranslateWithDynamicId();
        if (!Strings.ContainsKey(name))
            Strings[name] = (countPrefix, countSuffix);
        return name;
    }

    private static string GetFinalDescription(string name, int mentions)
        => !Strings.TryGetValue(name, out (string prefix, (string singular, string plural) suffix) strings)
            ? name
            : name + $" ({strings.prefix} {mentions} {(mentions == 1 ? strings.suffix.singular : strings.suffix.plural)})";

    private static bool TryGetSettlementFromName(string name, out Settlement settlement)
    {
        settlement = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (SettlementCache.TryGetValue(name, out settlement))
            return settlement is not null;
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
        policyObject = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (PolicyObjectCache.TryGetValue(name, out policyObject))
            return policyObject is not null;
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
        buildingType = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (BuildingTypesCache.TryGetValue(name, out buildingType))
            return buildingType is not null;
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
        itemCategory = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (ItemCategoryCache.TryGetValue(name, out itemCategory))
            return itemCategory is not null;
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

    private static bool TryGetPerkObjectFromName(string name, out PerkObject perkObject)
    {
        perkObject = null;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        if (PerkObjectCache.TryGetValue(name, out perkObject))
            return perkObject is not null;
        if (Campaign.Current is null || (perkObjects = PerkObject.All) is null)
            return false;
        foreach (PerkObject c in perkObjects.Where(c => c?.ToString() == name))
        {
            perkObject = c;
            PerkObjectCache[name] = perkObject;
            return true;
        }
        return false;
    }
}