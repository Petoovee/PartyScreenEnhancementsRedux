using System;
using System.ComponentModel;
using PartyScreenEnhancements.Saving;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Library;

namespace PartyScreenEnhancements.ViewModel
{
    public class UnitTallyVM : TaleWorlds.Library.ViewModel
    {
        private string _archersGarrisonLabel;
        private string _archersLabel;
        private string _cavalryGarrisonLabel;
        private string _cavalryLabel;
        private string _horseArcherGarrisonLabel;
        private string _horseArcherLabel;

        private string _infantryGarrisonLabel;
        private string _infantryLabel;

        private bool _isEnabled;
        private PartyScreenLogic _logic;
        private MBBindingList<PartyCharacterVM> _mainPartyList;
        private MBBindingList<PartyCharacterVM> _otherPartyList;
        private bool _shouldShowGarrison;

        public UnitTallyVM(MBBindingList<PartyCharacterVM> mainPartyList, MBBindingList<PartyCharacterVM> otherParty,
            PartyScreenLogic logic, bool shouldShowGarrison)
        {
            PartyScreenConfig.ExtraSettings.PropertyChanged += OnEnableChange;
            InfantryLabel = "Infantry: NaN";
            ArchersLabel = "Archers: NaN";
            CavalryLabel = "Cavalry: NaN";
            HorseArcherLabel = "Horse Archers: NaN";

            InfantryGarrisonLabel = "Infantry: NaN";
            ArchersGarrisonLabel = "Archers: NaN";
            CavalryGarrisonLabel = "Cavalry: NaN";
            _horseArcherGarrisonLabel = "Horse Archers: NaN";

            _mainPartyList = mainPartyList;
            _otherPartyList = otherParty;
            IsEnabled = PartyScreenConfig.ExtraSettings.DisplayCategoryNumbers;
            ShouldShowGarrison = shouldShowGarrison;

            _logic = logic;
            _logic.UpdateDelegate =
                Delegate.Combine(_logic.UpdateDelegate, new PartyScreenLogic.PresentationUpdate(RefreshDelegate)) as
                    PartyScreenLogic.PresentationUpdate;
        }

        [DataSourceProperty]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowGarrison
        {
            get => _shouldShowGarrison && _isEnabled;
            set
            {
                if (value != _shouldShowGarrison)
                {
                    _shouldShowGarrison = value;
                    OnPropertyChanged();
                }
            }
        }

        // Party Labels

        [DataSourceProperty]
        public string InfantryLabel
        {
            get => _infantryLabel;
            set
            {
                if (value != _infantryLabel)
                {
                    _infantryLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string ArchersLabel
        {
            get => _archersLabel;
            set
            {
                if (value != _archersLabel)
                {
                    _archersLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string CavalryLabel
        {
            get => _cavalryLabel;
            set
            {
                if (value != _cavalryLabel)
                {
                    _cavalryLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string HorseArcherLabel
        {
            get => _horseArcherLabel;
            set
            {
                if (value != _horseArcherLabel)
                {
                    _horseArcherLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        // Garrison Labels

        [DataSourceProperty]
        public string InfantryGarrisonLabel
        {
            get => _infantryGarrisonLabel;
            set
            {
                if (value != _infantryGarrisonLabel)
                {
                    _infantryGarrisonLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string ArchersGarrisonLabel
        {
            get => _archersGarrisonLabel;
            set
            {
                if (value != _archersGarrisonLabel)
                {
                    _archersGarrisonLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string CavalryGarrisonLabel
        {
            get => _cavalryGarrisonLabel;
            set
            {
                if (value != _cavalryGarrisonLabel)
                {
                    _cavalryGarrisonLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string HorseArcherGarrisonLabel
        {
            get => _horseArcherGarrisonLabel;
            set
            {
                if (value != _horseArcherGarrisonLabel)
                {
                    _horseArcherGarrisonLabel = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            _logic.UpdateDelegate =
                Delegate.Remove(_logic.UpdateDelegate, new PartyScreenLogic.PresentationUpdate(RefreshDelegate)) as
                    PartyScreenLogic.PresentationUpdate;
            PartyScreenConfig.ExtraSettings.PropertyChanged -= OnEnableChange;

            _mainPartyList = null;
            _otherPartyList = null;
            _logic = null;
        }

        public void RefreshDelegate(PartyScreenLogic.PartyCommand command)
        {
            RefreshValues();
        }

        public void OnEnableChange(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName.Equals(nameof(PartyScreenConfig.ExtraSettings
                    .DisplayCategoryNumbers)))
                IsEnabled = PartyScreenConfig.ExtraSettings.DisplayCategoryNumbers;
        }

        public new void RefreshValues()
        {
            try
            {
                base.RefreshValues();
                if (IsEnabled)
                {
                    int infantry = 0, archers = 0, cavalry = 0, horseArchers = 0;

                    foreach (var character in _mainPartyList)
                        if (character?.Character != null)
                        {
                            if (character.Character.IsMounted && character.Character.IsRanged)
                                horseArchers += character.Number;
                            else if (character.Character.IsMounted) cavalry += character.Number;
                            else if (character.Character.IsRanged) archers += character.Number;
                            else if (character.Character.IsInfantry) infantry += character.Number;
                        }

                    InfantryLabel = $"Infantry: {infantry}";
                    ArchersLabel = $"Archers: {archers}";
                    CavalryLabel = $"Cavalry: {cavalry}";
                    HorseArcherLabel = $"Horse Archers: {horseArchers}";
                }

                if (ShouldShowGarrison && _otherPartyList != null)
                {
                    int infantry = 0, archers = 0, cavalry = 0, horseArchers = 0;

                    foreach (var character in _otherPartyList)
                        if (character?.Character != null)
                        {
                            if (character.Character.IsMounted && character.Character.IsRanged)
                                horseArchers += character.Number;
                            else if (character.Character.IsMounted) cavalry += character.Number;
                            else if (character.Character.IsRanged) archers += character.Number;
                            else if (character.Character.IsInfantry) infantry += character.Number;
                        }

                    InfantryGarrisonLabel = $"Infantry: {infantry}";
                    ArchersGarrisonLabel = $"Archers: {archers}";
                    CavalryGarrisonLabel = $"Cavalry: {cavalry}";
                    HorseArcherGarrisonLabel = $"Horse Archers: {horseArchers}";
                }
            }
            catch (Exception e)
            {
                Logging.Log(Logging.Levels.ERROR, $"Unit Tally: {e}");
                Utilities.DisplayMessage($"PSE Unit Tally Label Update Exception {e}");
            }
        }
    }
}