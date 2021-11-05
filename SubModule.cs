using HarmonyLib;
using System;
using TaleWorlds.Core;
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
                HarmonyMethod harmonyMethod = new HarmonyMethod(typeof(Sorting), nameof(Sorting.SorterPatch));
                Type defaultClanFinanceModel = AccessTools.TypeByName("TaleWorlds.CampaignSystem.SandBox.GameComponents.DefaultClanFinanceModel");
                harmony.Patch(original: AccessTools.Method(defaultClanFinanceModel, "CalculateClanGoldChange"), postfix: harmonyMethod);
                harmony.Patch(original: AccessTools.Method(defaultClanFinanceModel, "CalculateClanIncome"), postfix: harmonyMethod);
                harmony.Patch(original: AccessTools.Method(defaultClanFinanceModel, "CalculateClanExpenses"), postfix: harmonyMethod);
                Type garrisonCostModel = AccessTools.TypeByName("ImprovedGarrisons.Models.GarrisonCostModel");
                if (!(garrisonCostModel is null))
                {
                    harmony.Patch(original: AccessTools.Method(garrisonCostModel, "CalculateClanGoldChange"), postfix: harmonyMethod);
                    harmony.Patch(original: AccessTools.Method(garrisonCostModel, "CalculateClanExpenses"), postfix: harmonyMethod);
                    InformationManager.DisplayMessage(new InformationMessage("Sorted Income patched for Improved Garrisons", Colors.Yellow, "SortedIncome"));
                }
                InformationManager.DisplayMessage(new InformationMessage("Sorted Income initialized", Colors.Yellow, "SortedIncome"));
            }
        }
    }
}