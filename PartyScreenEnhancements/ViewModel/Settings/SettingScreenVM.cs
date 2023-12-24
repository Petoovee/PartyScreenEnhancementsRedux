using PartyScreenEnhancements.Saving;
using PartyScreenEnhancements.ViewModel.Settings.Tabs;
using PartyScreenEnhancements.ViewModel.Settings.Tabs.Miscellaneous;
using SandBox.GauntletUI;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;

namespace PartyScreenEnhancements.ViewModel.Settings
{
    /// <summary>
    /// Primary VM for the Setting Screen.
    /// Used to call OnFinalize for all child VMs
    /// </summary>
    public class SettingScreenVM : TaleWorlds.Library.ViewModel
    {

        private PartyEnhancementsVM _partyEnhancementsVm;

        private SettingGeneralPaneVM _generalPane;
        private SettingMiscPaneVM _miscPane;

        private GauntletLayer _subSettingLayer;
        private GauntletPartyScreen _parentScreen;
        private IGauntletMovie _currentMovie;

        public SettingScreenVM(PartyEnhancementsVM parent, GauntletPartyScreen parentScreen)
        {
            _partyEnhancementsVm = parent;
            _parentScreen = parentScreen;
            _generalPane = new SettingGeneralPaneVM();
            _miscPane = new SettingMiscPaneVM();

            if (Game.Current != null)
                Game.Current.AfterTick = (Action<float>)Delegate.Combine(Game.Current.AfterTick, new Action<float>(AfterTick));
        }

        public void AfterTick(float dt)
        {
            if (_partyEnhancementsVm.IsHotKeyPressed("Exit"))
            {
                ExecuteCloseSettings();
            }
        }

        public void ExecuteCloseSettings()
        {
            _partyEnhancementsVm.CloseSettingView();
            _generalPane.OnFinalize();
            _miscPane.OnFinalize();
            OnFinalize();
        }

        public void CloseSubSetting()
        {
            if (_subSettingLayer != null)
            {
                _subSettingLayer.ReleaseMovie(_currentMovie);
                _parentScreen.RemoveLayer(_subSettingLayer);
                _subSettingLayer.InputRestrictions.ResetInputRestrictions();
                _subSettingLayer = null;
                RefreshValues();
            }
        }

        public new void OnFinalize()
        {
            base.OnFinalize();
            PartyScreenConfig.Save();
            if (Game.Current != null)
                Game.Current.AfterTick = (Action<float>)Delegate.Remove(Game.Current.AfterTick, new Action<float>(AfterTick));

            _partyEnhancementsVm = null;
            _generalPane = null;
        }

        [DataSourceProperty]
        public SettingGeneralPaneVM GeneralPane
        {
            get => _generalPane;
            set
            {
                if (value != _generalPane)
                {
                    _generalPane = value;
                    base.OnPropertyChanged(nameof(GeneralPane));
                }
            }
        }

        [DataSourceProperty]
        public SettingMiscPaneVM MiscPane
        {
            get => _miscPane;
            set
            {
                if (value != _miscPane)
                {
                    _miscPane = value;
                    base.OnPropertyChanged(nameof(MiscPane));
                }
            }
        }
    }
}
