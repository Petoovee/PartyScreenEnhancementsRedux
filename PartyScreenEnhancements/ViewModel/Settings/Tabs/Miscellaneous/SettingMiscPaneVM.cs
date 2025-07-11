﻿using PartyScreenEnhancements.Saving;
using TaleWorlds.Library;

namespace PartyScreenEnhancements.ViewModel.Settings.Tabs.Miscellaneous
{
    public class SettingMiscPaneVM : TaleWorlds.Library.ViewModel
    {
        public SettingMiscPaneVM()
        {
            Name = "Miscellaneous";
        }

        [DataSourceProperty] public string Name { get; set; }

        public void ExecuteResetUpgradePaths()
        {
            InformationManager.DisplayMessage(new InformationMessage("Cleared Upgrade Paths!"));
            PartyScreenConfig.PathsToUpgrade.Clear();
            PartyScreenConfig.Save();
        }

        public void ExecuteResetPrisonerPaths()
        {
            InformationManager.DisplayMessage(new InformationMessage("Cleared Prisoner Paths!"));
            PartyScreenConfig.PrisonersToRecruit.Clear();
            PartyScreenConfig.Save();
        }
    }
}