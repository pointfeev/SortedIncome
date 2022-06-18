using System;

using HarmonyLib;

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
                Type explainedNumber = AccessTools.TypeByName("TaleWorlds.CampaignSystem.ExplainedNumber");
                harmony.Patch(original: AccessTools.Method(explainedNumber, "GetLines"), postfix: explainedNumberPatch);
                InformationManager.DisplayMessage(new InformationMessage("Sorted Income initialized", Colors.Yellow, "SortedIncome"));
            }
        }
    }
}