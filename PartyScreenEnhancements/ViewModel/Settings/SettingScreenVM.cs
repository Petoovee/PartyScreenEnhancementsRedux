using System;
using PartyScreenEnhancements.Saving;
using PartyScreenEnhancements.ViewModel.Settings.SortingOrders;
using PartyScreenEnhancements.ViewModel.Settings.Tabs;
using PartyScreenEnhancements.ViewModel.Settings.Tabs.Miscellaneous;
using PartyScreenEnhancements.ViewModel.Settings.Tabs.Sorting;
using SandBox.GauntletUI;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace PartyScreenEnhancements.ViewModel.Settings
{
    /// <summary>
    ///     Primary VM for the Setting Screen.
    ///     Used to call OnFinalize for all child VMs
    /// </summary>
    public class SettingScreenVM : TaleWorlds.Library.ViewModel
    {
        private readonly GauntletPartyScreen _parentScreen;
        private IGauntletMovie _currentMovie;
        private SettingGeneralPaneVM _generalPane;
        private SettingMiscPaneVM _miscPane;

        private PartyEnhancementsVM _partyEnhancementsVm;

        private SettingSorterOverlayVM _sorterPane;
        private SettingSortingOrderScreenVM _subScreen;

        private GauntletLayer _subSettingLayer;

        public SettingScreenVM(PartyEnhancementsVM parent, GauntletPartyScreen parentScreen)
        {
            _partyEnhancementsVm = parent;
            _parentScreen = parentScreen;
            _sorterPane = new SettingSorterOverlayVM(this);
            _generalPane = new SettingGeneralPaneVM();
            _miscPane = new SettingMiscPaneVM();

            if (Game.Current != null)
                Game.Current.AfterTick =
                    (Action<float>)Delegate.Combine(Game.Current.AfterTick, new Action<float>(AfterTick));
        }

        [DataSourceProperty]
        public SettingSorterOverlayVM SorterPane
        {
            get => _sorterPane;
            set
            {
                if (value != _sorterPane)
                {
                    _sorterPane = value;
                    OnPropertyChanged();
                }
            }
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }

        public void AfterTick(float dt)
        {
            if (_partyEnhancementsVm.IsHotKeyPressed("Exit")) ExecuteCloseSettings();
            if (_subSettingLayer != null && _subSettingLayer.Input.IsHotKeyReleased("Exit"))
                _subScreen.ExecuteCloseSettings();
        }

        public void ExecuteCloseSettings()
        {
            _partyEnhancementsVm.CloseSettingView();
            _sorterPane.OnFinalize();
            _generalPane.OnFinalize();
            _miscPane.OnFinalize();
            OnFinalize();
        }

        public void OpenSubSetting(SettingSortVM sortVm)
        {
            if (_subSettingLayer == null)
            {
                _subSettingLayer = new GauntletLayer(300);
                _subScreen = new SettingSortingOrderScreenVM(this, sortVm.SortingComparer);
                _currentMovie = _subSettingLayer.LoadMovie("PartyEnhancementSortingSettings", _subScreen);
                _subSettingLayer.IsFocusLayer = true;
                ScreenManager.TrySetFocus(_subSettingLayer);
                _subSettingLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
                _parentScreen.AddLayer(_subSettingLayer);
                _subSettingLayer.InputRestrictions.SetInputRestrictions();
            }
        }

        public void CloseSubSetting()
        {
            if (_subSettingLayer != null)
            {
                _subSettingLayer.ReleaseMovie(_currentMovie);
                _parentScreen.RemoveLayer(_subSettingLayer);
                _subSettingLayer.InputRestrictions.ResetInputRestrictions();
                _subSettingLayer = null;
                _subScreen = null;
                RefreshValues();
            }
        }

        public new void OnFinalize()
        {
            base.OnFinalize();
            PartyScreenConfig.Save();
            if (Game.Current != null)
                Game.Current.AfterTick =
                    (Action<float>)Delegate.Remove(Game.Current.AfterTick, new Action<float>(AfterTick));

            _partyEnhancementsVm = null;
            _sorterPane = null;
            _generalPane = null;
        }
    }
}