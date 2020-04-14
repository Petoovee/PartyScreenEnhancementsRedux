﻿using HarmonyLib;
using PartyScreenEnhancements.ViewModel;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace PartyScreenEnhancements.Patches
{
    [HarmonyPatch(typeof(ScreenBase))]
    public class PartyEnhancementLayerPatch
    {
        internal static GauntletLayer screenLayer;

        [HarmonyPatch("AddLayer")]
        public static void Postfix(ref ScreenBase __instance)
        {
            if(__instance is GauntletPartyScreen partyScreen && screenLayer == null)
            {
                screenLayer = new GauntletLayer(1);

                var traverser = Traverse.Create(partyScreen);
                var partyVM = traverser.Field<PartyVM>("_dataSource").Value;
                var partyState = traverser.Field<PartyState>("_partyState").Value;

                var enhancementVM = new PartyEnhancementsVM(partyVM, partyState.PartyScreenLogic);
                screenLayer.LoadMovie("PartyScreenEnhancements", enhancementVM);
                screenLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                partyScreen.AddLayer(screenLayer);
            }
        }

        [HarmonyPatch("RemoveLayer")]
        public static void Prefix(ref ScreenBase __instance)
        {
            if (__instance is GauntletPartyScreen partyScreen && screenLayer != null)
            {
                screenLayer.InputRestrictions.ResetInputRestrictions();
                partyScreen.RemoveLayer(screenLayer);
                ScreenManager.TryLoseFocus(screenLayer);
                screenLayer = null;
            }
        }
    }
}