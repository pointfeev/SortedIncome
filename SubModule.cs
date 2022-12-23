using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SortedIncome
{
    public class SubModule : MBSubModuleBase
    {
        private bool harmonyPatched;

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (harmonyPatched)
                return;
            harmonyPatched = true;
            Harmony harmony = new Harmony("pointfeev.sortedincome");
            HarmonyMethod preAdd = new HarmonyMethod(typeof(Sorting), nameof(Sorting.ExplainedNumberPrefix));
            _ = harmony.Patch(AccessTools.Method(typeof(ExplainedNumber), nameof(ExplainedNumber.Add)), preAdd);
            HarmonyMethod postAdd = new HarmonyMethod(typeof(Sorting), nameof(Sorting.ExplainedNumberPostfix));
            _ = harmony.Patch(AccessTools.Method(typeof(ExplainedNumber), nameof(ExplainedNumber.Add)),
                              postfix: postAdd);
            HarmonyMethod begin = new HarmonyMethod(typeof(Sorting), nameof(Sorting.BeginTooltip));
            _ = harmony.Patch(
                AccessTools.Method(typeof(BasicTooltipViewModel), nameof(BasicTooltipViewModel.ExecuteBeginHint)),
                postfix: begin);
            HarmonyMethod show = new HarmonyMethod(typeof(Sorting), nameof(Sorting.ShowTooltip));
            _ = harmony.Patch(
                AccessTools.Method(typeof(PropertyBasedTooltipVM), nameof(PropertyBasedTooltipVM.OnShowTooltip)),
                postfix: show);
            HarmonyMethod tick = new HarmonyMethod(typeof(Sorting), nameof(Sorting.TickTooltip));
            _ = harmony.Patch(AccessTools.Method(typeof(PropertyBasedTooltipVM), nameof(PropertyBasedTooltipVM.Tick)),
                              postfix: tick);
            HarmonyMethod get = new HarmonyMethod(typeof(Sorting), nameof(Sorting.GetTooltip));
            _ = harmony.Patch(AccessTools.Method(typeof(CampaignUIHelper),
                                                 nameof(CampaignUIHelper.GetTooltipForAccumulatingProperty)),
                              postfix: get);
            _ = harmony.Patch(AccessTools.Method(typeof(CampaignUIHelper),
                                                 nameof(CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult)),
                              postfix: get);
            Sorting.AddLine
                = AccessTools.Method(AccessTools.TypeByName("TaleWorlds.CampaignSystem.ExplainedNumber+StatExplainer"),
                                     "AddLine");
            if (Sorting.AddLine == null)
                InformationManager.DisplayMessage(new InformationMessage(
                                                      "Sorted Income failed to get StatExplainer.AddLine method!",
                                                      Colors.Red, "SortedIncome"));
            Sorting.Value = AccessTools.Field(AccessTools.TypeByName("TaleWorlds.Localization.TextObject"), "Value");
            if (Sorting.Value == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                                                      "Sorted Income failed to get TextObject.Value field!", Colors.Red,
                                                      "SortedIncome"));
            }
            else
            {
                IEnumerable<FieldInfo> fields = AccessTools.GetDeclaredFields(
                    AccessTools.TypeByName("TaleWorlds.CampaignSystem.GameComponents.DefaultClanFinanceModel"));
                foreach (FieldInfo field in fields)
                    if (field.FieldType == typeof(TextObject))
                        Sorting.TextObjectStrs[field.Name.Trim(' ', '_').Replace("Str", "")]
                            = (string)Sorting.Value.GetValue((TextObject)field.GetValue(null));
                if (!Sorting.TextObjectStrs.Any())
                    InformationManager.DisplayMessage(new InformationMessage(
                                                          "Sorted Income failed to gather any TextObjectStrs!",
                                                          Colors.Red,
                                                          "SortedIncome"));
            }
            if (Sorting.AddLine != null && Sorting.Value != null && Sorting.TextObjectStrs.Any())
                InformationManager.DisplayMessage(
                    new InformationMessage("Sorted Income initialized", Colors.Yellow, "SortedIncome"));
            else
                InformationManager.DisplayMessage(
                    new InformationMessage("Sorted Income failed to initialize", Colors.Red, "SortedIncome"));
        }
    }
}