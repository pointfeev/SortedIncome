
using HarmonyLib;

using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SortedIncome
{
    public class SubModule : MBSubModuleBase
    {
        private bool harmonyPatched = false;

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (!harmonyPatched)
            {
                harmonyPatched = true;
                Harmony harmony = new Harmony("pointfeev.sortedincome");
                HarmonyMethod begin = new HarmonyMethod(typeof(Sorting), nameof(Sorting.BeginTooltip));
                _ = harmony.Patch(original: AccessTools.Method(typeof(BasicTooltipViewModel), nameof(BasicTooltipViewModel.ExecuteBeginHint)), postfix: begin);
                HarmonyMethod show = new HarmonyMethod(typeof(Sorting), nameof(Sorting.ShowTooltip));
                _ = harmony.Patch(original: AccessTools.Method(typeof(PropertyBasedTooltipVM), nameof(PropertyBasedTooltipVM.OnShowTooltip)), postfix: show);
                HarmonyMethod tick = new HarmonyMethod(typeof(Sorting), nameof(Sorting.TickTooltip));
                _ = harmony.Patch(original: AccessTools.Method(typeof(PropertyBasedTooltipVM), nameof(PropertyBasedTooltipVM.Tick)), postfix: tick);
                HarmonyMethod get = new HarmonyMethod(typeof(Sorting), nameof(Sorting.GetTooltip));
                _ = harmony.Patch(original: AccessTools.Method(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetTooltipForAccumulatingProperty)), postfix: get);
                _ = harmony.Patch(original: AccessTools.Method(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetTooltipForAccumulatingPropertyWithResult)), postfix: get);
                InformationManager.DisplayMessage(new InformationMessage("Sorted Income initialized", Colors.Yellow, "SortedIncome"));
            }
        }
    }
}