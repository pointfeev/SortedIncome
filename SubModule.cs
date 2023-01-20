using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SortedIncome.Utilities;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SortedIncome;

public class SubModule : MBSubModuleBase
{
    private bool initialized;

    protected override void OnBeforeInitialModuleScreenSetAsRoot()
    {
        if (initialized)
            return;
        initialized = true;
        List<string> failures = new();
        Sorting.Value = AccessTools.Field(typeof(TextObject), "Value");
        if (Sorting.Value == null)
            failures.Add("Failed to get TextObject.Value field");
        else
        {
            IEnumerable<FieldInfo> fields = AccessTools.GetDeclaredFields(typeof(DefaultClanFinanceModel));
            foreach (FieldInfo field in fields)
                if (field.FieldType == typeof(TextObject))
                    Sorting.ModelTextValues.Add(field.Name.Trim(' ', '_').Replace("Str", ""), (string)Sorting.Value.GetValue((TextObject)field.GetValue(null)));
            if (Sorting.ModelTextValues.Count == 0)
                failures.Add("Failed to get any DefaultClanFinanceModel TextObject fields");
        }
        Type statExplainer = AccessTools.TypeByName(typeof(ExplainedNumber).FullName + "+StatExplainer");
        if (statExplainer == null)
            failures.Add("Failed to get ExplainedNumber+StatExplainer type");
        else
        {
            Sorting.AddLine = AccessTools.Method(statExplainer, "AddLine");
            if (Sorting.AddLine == null)
                failures.Add("Failed to get StatExplainer.AddLine method");
            Type operationType = AccessTools.TypeByName(statExplainer.FullName + "+OperationType");
            if (operationType != null)
                try
                {
                    Sorting.OperationType = Enum.ToObject(operationType, 1);
                }
                catch
                {
                    // ignore
                }
            if (Sorting.OperationType == null)
                failures.Add("Failed to get StatExplainer+OperationType enum");
        }
        if (failures.Count > 0)
        {
            OutputUtils.DoOutput(string.Join("\n", failures), title: " failed to initialize");
            return;
        }
        Harmony harmony = new("pointfeev.sortedincome");
        HarmonyMethod add = new(typeof(Sorting), nameof(Sorting.AddTooltip));
        _ = harmony.Patch(AccessTools.Method(typeof(ExplainedNumber), nameof(ExplainedNumber.Add)), add);
        HarmonyMethod begin = new(typeof(Sorting), nameof(Sorting.BeginTooltip));
        _ = harmony.Patch(AccessTools.Method(typeof(BasicTooltipViewModel), nameof(BasicTooltipViewModel.ExecuteBeginHint)), postfix: begin);
        HarmonyMethod show = new(typeof(Sorting), nameof(Sorting.ShowTooltip));
        _ = harmony.Patch(AccessTools.Method(typeof(PropertyBasedTooltipVM), nameof(PropertyBasedTooltipVM.OnShowTooltip)), postfix: show);
        HarmonyMethod tick = new(typeof(Sorting), nameof(Sorting.TickTooltip));
        _ = harmony.Patch(AccessTools.Method(typeof(PropertyBasedTooltipVM), nameof(PropertyBasedTooltipVM.Tick)), postfix: tick);
        HarmonyMethod get = new(typeof(Sorting), nameof(Sorting.GetTooltip));
        _ = harmony.Patch(AccessTools.Method(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetTooltipForAccumulatingProperty)), postfix: get);
        _ = harmony.Patch(AccessTools.Method(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult)), postfix: get);
    }
}