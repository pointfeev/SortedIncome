using System;

using HarmonyLib;

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
                HarmonyMethod sorterPatchDenars = new HarmonyMethod(typeof(Sorting), nameof(Sorting.SorterPatchDenars));
                Type defaultClanFinanceModel = AccessTools.TypeByName("TaleWorlds.CampaignSystem.SandBox.GameComponents.DefaultClanFinanceModel");
                harmony.Patch(original: AccessTools.Method(defaultClanFinanceModel, "CalculateClanGoldChange"), postfix: sorterPatchDenars);
                harmony.Patch(original: AccessTools.Method(defaultClanFinanceModel, "CalculateClanIncome"), postfix: sorterPatchDenars);
                harmony.Patch(original: AccessTools.Method(defaultClanFinanceModel, "CalculateClanExpenses"), postfix: sorterPatchDenars);
                Type improvedGarrisonsCostModel = AccessTools.TypeByName("ImprovedGarrisons.Models.GarrisonCostModel");
                if (!(improvedGarrisonsCostModel is null))
                {
                    harmony.Patch(original: AccessTools.Method(improvedGarrisonsCostModel, "CalculateClanGoldChange"), postfix: sorterPatchDenars);
                    harmony.Patch(original: AccessTools.Method(improvedGarrisonsCostModel, "CalculateClanExpenses"), postfix: sorterPatchDenars);
                    InformationManager.DisplayMessage(new InformationMessage("Sorted Income patched for Improved Garrisons", Colors.Yellow, "SortedIncome"));
                }
                HarmonyMethod sorterPatchInfluence = new HarmonyMethod(typeof(Sorting), nameof(Sorting.SorterPatchInfluence));
                Type defaultClanPoliticsModel = AccessTools.TypeByName("TaleWorlds.CampaignSystem.SandBox.GameComponents.DefaultClanPoliticsModel");
                harmony.Patch(original: AccessTools.Method(defaultClanPoliticsModel, "CalculateInfluenceChange"), postfix: sorterPatchInfluence);
                Type populationsOfCalradiaInfluenceModel = AccessTools.TypeByName("Populations.Models.InfluenceModel");
                if (!(populationsOfCalradiaInfluenceModel is null))
                {
                    harmony.Patch(original: AccessTools.Method(populationsOfCalradiaInfluenceModel, "CalculateInfluenceChange"), postfix: sorterPatchInfluence);
                    InformationManager.DisplayMessage(new InformationMessage("Sorted Income patched for Populations of Calradia", Colors.Yellow, "SortedIncome"));
                }
                InformationManager.DisplayMessage(new InformationMessage("Sorted Income initialized", Colors.Yellow, "SortedIncome"));
            }
        }
    }
}