﻿using HarmonyLib;
using PartyScreenEnhancements.Saving;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.ScreenSystem;

namespace PartyScreenEnhancements.Patches
{
    [HarmonyPatch(typeof(UpgradeTargetVM), "Refresh")]
    public class UpgradeButtonTooltipPatch
    {
        private const string UPGRADE_TOOLTIP = "\nHold [CTRL] and [SHIFT] to select as preferred upgrade path";

        public static void Prefix(ref string hint)
        {
            if (ScreenManager.TopScreen is GauntletPartyScreen && PartyScreenConfig.ExtraSettings.PathSelectTooltips)
                hint += UPGRADE_TOOLTIP;
        }
    }
}