
using HarmonyLib;

using TaleWorlds.CampaignSystem.ViewModelCollection;
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
                _ = harmony.Patch(original: AccessTools.Method(typeof(CampaignUIHelper), nameof(CampaignUIHelper.GetTooltipForAccumulatingProperty)),
                                  postfix: new HarmonyMethod(typeof(Sorting), nameof(Sorting.GetTooltipForAccumulatingProperty)));
                InformationManager.DisplayMessage(new InformationMessage("Sorted Income initialized", Colors.Yellow, "SortedIncome"));
            }
        }
    }
}