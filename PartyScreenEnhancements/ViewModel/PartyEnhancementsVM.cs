﻿using System;
using System.ComponentModel;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using PartyScreenEnhancements.Saving;
using PartyScreenEnhancements.ViewModel.Settings;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party.PartyTroopManagerPopUp;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace PartyScreenEnhancements.ViewModel
{
    /// <summary>
    ///     Primary VM for the overlay (includes buttons, unit tallies, etc)
    ///     Holds references to all other VMs relevant
    /// </summary>
    public class PartyEnhancementsVM : TaleWorlds.Library.ViewModel
    {
        private readonly GauntletPartyScreen _parentScreen;
        private readonly PartyScreenLogic _partyScreenLogic;
        private readonly PartyVM _partyVM;
        private GauntletLayer _popupLayer;
        private IGauntletMovie _popupMovie;
        private RecruitPrisonerVM _recruitPrisonerVm;
        private GauntletLayer _settingLayer;
        private IGauntletMovie _settingMovie;
        private SettingScreenVM _settingScreenVm;

        private HintViewModel _settingsHint;

        private SortAllTroopsVM _sortTroopsVM;
        private TransferWoundedTroopsVM _transferWounded;
        private UnitTallyVM _unitTallyVm;
        private UpgradeAllTroopsVM _upgradeTroopsVM;


        public PartyEnhancementsVM(PartyVM partyVM, PartyScreenLogic partyScreenLogic, GauntletPartyScreen parentScreen)
        {
            _partyVM = partyVM;
            _partyScreenLogic = partyScreenLogic;
            _parentScreen = parentScreen;
            _settingsHint = new HintViewModel(new TextObject("PSE Settings"));

            _sortTroopsVM = new SortAllTroopsVM(_partyVM, _partyScreenLogic);
            _upgradeTroopsVM = new UpgradeAllTroopsVM(this, _partyVM, _partyScreenLogic);
            _recruitPrisonerVm = new RecruitPrisonerVM(this, _partyVM, _partyScreenLogic);
            _unitTallyVm = new UnitTallyVM(partyVM.MainPartyTroops, partyVM.OtherPartyTroops, partyScreenLogic,
                _partyScreenLogic?.LeftOwnerParty?.MobileParty?.IsGarrison ?? false);
            _transferWounded = new TransferWoundedTroopsVM(this, partyVM,
                _partyScreenLogic?.LeftOwnerParty?.MobileParty?.IsGarrison ?? false);

            _partyScreenLogic.AfterReset += AfterReset;
            _partyScreenLogic.Update += UpdateLabel;
            PartyScreenConfig.ExtraSettings.PropertyChanged += OnEnableChange;

            RefreshValues();
            UpdateLabel(null);
        }

        [DataSourceProperty] public bool AnyOtherPopupOpen => _partyVM.IsAnyPopUpOpen;

        [DataSourceProperty]
        public HintViewModel SettingHint
        {
            get => _settingsHint;
            set
            {
                if (value != _settingsHint)
                {
                    _settingsHint = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public UpgradeAllTroopsVM UpgradeAllTroops
        {
            get => _upgradeTroopsVM;
            set
            {
                if (value != _upgradeTroopsVM)
                {
                    _upgradeTroopsVM = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public RecruitPrisonerVM RecruitAllPrisoners
        {
            get => _recruitPrisonerVm;
            set
            {
                if (value != _recruitPrisonerVm)
                {
                    _recruitPrisonerVm = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public SortAllTroopsVM SortAllTroops
        {
            get => _sortTroopsVM;
            set
            {
                if (value != _sortTroopsVM)
                {
                    _sortTroopsVM = value;
                    OnPropertyChanged();
                }
            }
        }


        [DataSourceProperty]
        public UnitTallyVM UnitTally
        {
            get => _unitTallyVm;
            set
            {
                if (value != _unitTallyVm)
                {
                    _unitTallyVm = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public TransferWoundedTroopsVM TransferWoundedTroops
        {
            get => _transferWounded;
            set
            {
                if (value != _transferWounded)
                {
                    _transferWounded = value;
                    OnPropertyChanged();
                }
            }
        }

        public void UpdateLabel([CanBeNull] PartyScreenLogic.PartyCommand command)
        {
            try
            {
                if (!PartyScreenConfig.ExtraSettings.ShouldShowCompletePartyNumber) return;

                var _otherParty = _partyVM.OtherPartyTroops;
                var _mainParty = _partyVM.MainPartyTroops;

                if (_mainParty != null && !_mainParty.IsEmpty() &&
                    _partyScreenLogic.RightOwnerParty?.PartySizeLimit > 0)
                    _partyVM.MainPartyTroopsLbl =
                        PopulatePartyList(_mainParty, _partyScreenLogic.RightOwnerParty.PartySizeLimit);

                if (_otherParty != null && !_otherParty.IsEmpty() &&
                    _partyScreenLogic.LeftOwnerParty?.PartySizeLimit > 0)
                    _partyVM.OtherPartyTroopsLbl =
                        PopulatePartyList(_otherParty, _partyScreenLogic.LeftOwnerParty.PartySizeLimit);
            }
            catch (Exception e)
            {
                Logging.Log(Logging.Levels.ERROR, $"Update Label: {e}");
                Utilities.DisplayMessage($"PSE UpdateLabel Exception: {e}");
            }
        }

        private string PopulatePartyList(MBBindingList<PartyCharacterVM> list, int sizeLimit)
        {
            var troopNumb = list.Sum(character => Math.Max(0, character.Troop.Number));
            return $"({troopNumb} / {sizeLimit})";
        }

        public void AfterReset(PartyScreenLogic logic, bool fromCancel)
        {
            RefreshValues();
        }

        public new void RefreshValues()
        {
            base.RefreshValues();

            if (PartyScreenConfig.ExtraSettings.AutomaticSorting) _sortTroopsVM.SortTroops();
            UpdateLabel(null);

            _unitTallyVm.RefreshValues();
        }

        public new void OnFinalize()
        {
            _partyScreenLogic.AfterReset -= AfterReset;
            _partyScreenLogic.Update -= UpdateLabel;
            PartyScreenConfig.ExtraSettings.PropertyChanged -= OnEnableChange;

            _unitTallyVm.OnFinalize();
            _recruitPrisonerVm.OnFinalize();
            _upgradeTroopsVM.OnFinalize();
            _sortTroopsVM.OnFinalize();

            _unitTallyVm = null;
            _recruitPrisonerVm = null;
            _upgradeTroopsVM = null;
            _sortTroopsVM = null;
        }

        /*
         * Screen layer methods
         */

        public void OpenPopupViewEnhancements(PartyTroopManagerVM openedPopup)
        {
            if (_popupLayer != null) return;

            _popupLayer = new GauntletLayer(200);

            if (openedPopup is PartyUpgradeTroopVM)
                _popupMovie = _popupLayer.LoadMovie("PSEUpgradePopup", _upgradeTroopsVM);
            else if (openedPopup is PartyRecruitTroopVM)
                _popupMovie = _popupLayer.LoadMovie("PSERecruitPopup", _recruitPrisonerVm);

            _parentScreen.AddLayer(_popupLayer);
            _popupLayer.InputRestrictions.SetInputRestrictions();
        }

        public void ClosePopupViewEnhancements()
        {
            if (_popupLayer != null)
            {
                _popupLayer.ReleaseMovie(_popupMovie);
                _parentScreen.RemoveLayer(_popupLayer);
                _popupLayer.InputRestrictions.ResetInputRestrictions();
                _popupLayer = null;
            }
        }

        public void OpenSettingView()
        {
            if (_settingLayer == null)
            {
                _settingLayer = new GauntletLayer(200);
                _settingScreenVm = new SettingScreenVM(this, _parentScreen);
                _settingMovie = _settingLayer.LoadMovie("PartyEnhancementSettings", _settingScreenVm);
                _settingLayer.IsFocusLayer = true;
                ScreenManager.TrySetFocus(_settingLayer);
                _settingLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
                _parentScreen.AddLayer(_settingLayer);
                _settingLayer.InputRestrictions.SetInputRestrictions();
            }
        }

        public void CloseSettingView()
        {
            if (_settingLayer != null)
            {
                _settingLayer.ReleaseMovie(_settingMovie);
                _parentScreen.RemoveLayer(_settingLayer);
                _settingLayer.InputRestrictions.ResetInputRestrictions();
                _settingLayer = null;
                _settingScreenVm = null;
                RefreshValues();
            }
        }

        /*
         * Misc Methods
         */

        public bool IsHotKeyPressed(string hotkey)
        {
            if (_settingLayer != null) return _settingLayer.Input.IsHotKeyReleased(hotkey);

            return false;
        }

        public void OnEnableChange(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName.Equals(nameof(PartyScreenConfig.ExtraSettings
                    .ShouldShowCompletePartyNumber)))
            {
                Traverse.Create(_partyVM).Method("RefreshPartyInformation").GetValue();
                UpdateLabel(null);
            }
        }
    }
}