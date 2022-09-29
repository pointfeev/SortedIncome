
using HarmonyLib;

using TaleWorlds.CampaignSystem;
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
                HarmonyMethod explainedNumberPatch = new HarmonyMethod(typeof(Sorting), nameof(Sorting.Patch));
                _ = harmony.Patch(original: AccessTools.Method(typeof(ExplainedNumber), "GetLines"), postfix: explainedNumberPatch);
                InformationManager.DisplayMessage(new InformationMessage("Sorted Income initialized", Colors.Yellow, "SortedIncome"));
            }
        }
    }
}