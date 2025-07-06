using System;
using PartyScreenEnhancements.Comparers;
using PartyScreenEnhancements.Patches;
using PartyScreenEnhancements.Saving;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace PartyScreenEnhancements.ViewModel
{
    public class SortAllTroopsVM : TaleWorlds.Library.ViewModel
    {
        private const int _leftSide = (int)PartyScreenLogic.PartyRosterSide.Left;
        private const int _rightSide = (int)PartyScreenLogic.PartyRosterSide.Right;
        private MBBindingList<PartyCharacterVM> _mainPartyList;
        private MBBindingList<PartyCharacterVM> _mainPartyPrisoners;
        private PartyScreenLogic _partyLogic;
        private PartyVM _partyVM;

        private HintViewModel _sortHint;

        public SortAllTroopsVM(PartyVM partyVm, PartyScreenLogic logic)
        {
            _partyVM = partyVm;
            _partyLogic = logic;
            _mainPartyList = _partyVM.MainPartyTroops;
            _mainPartyPrisoners = _partyVM.MainPartyPrisoners;
            _sortHint = new HintViewModel(new TextObject("Sort Troops\nCtrl Click to sort just main party"));
        }

        [DataSourceProperty]
        public HintViewModel SortHint
        {
            get => _sortHint;
            set
            {
                if (value != _sortHint)
                {
                    _sortHint = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            _mainPartyPrisoners = null;
            _mainPartyList = null;
            _partyLogic = null;
            _partyVM = null;
        }

        public void SortTroops()
        {
            var settings = PartyScreenConfig.ExtraSettings;

            try
            {
                SortAnyParty(_mainPartyList, _partyLogic.RightOwnerParty, _partyLogic.MemberRosters[_rightSide],
                    settings.PartySorter);

                if (!Utilities.IsControlDown())
                {
                    SortAnyParty(_mainPartyPrisoners,
                        null,
                        _partyLogic.PrisonerRosters[_rightSide],
                        settings.SeparateSortingProfiles ? settings.PrisonerSorter : settings.PartySorter);

                    SortAnyParty(_partyVM.OtherPartyPrisoners,
                        null,
                        _partyLogic.PrisonerRosters[_leftSide],
                        settings.SeparateSortingProfiles ? settings.PrisonerSorter : settings.PartySorter);

                    if (_partyLogic.LeftOwnerParty?.MobileParty != null)
                    {
                        var useGarrisonSorter = settings.SeparateSortingProfiles;
                        var sorterToUse = useGarrisonSorter
                            ? settings.GarrisonAndAlliedPartySorter
                            : settings.PartySorter;

                        SortAnyParty(_partyVM.OtherPartyTroops, _partyLogic.LeftOwnerParty,
                            _partyLogic.MemberRosters[_leftSide], sorterToUse);
                    }
                }

                if (!_mainPartyList.IsEmpty() && (!_mainPartyList[0]?.Troop.Character?.IsPlayerCharacter ?? false))
                    InformationManager.DisplayMessage(new InformationMessage(
                        "Your player character is no longer at the top of the list due to sorting, do NOT save your game and notify the mod manager"));
            }
            catch (Exception e)
            {
                Logging.Log(Logging.Levels.ERROR, $"Sorting: {e}");
                Utilities.DisplayMessage($"PSE Sorting Unit Exception: {e}");
            }
        }

        private static void SortAnyParty(MBBindingList<PartyCharacterVM> toSort, PartyBase party,
            TroopRoster rosterToSort, PartySort sorter)
        {
            if (rosterToSort == null || rosterToSort.Count == 0 || toSort == null || toSort.IsEmpty()) return;

            var leaderOfParty = party?.LeaderHero?.CharacterObject;

            // Sort the list, this is done for the visual unit cards to be properly positioned after the sort
            // This is not yet persisted to the actual roster, that is done after this.

            toSort.StableSort(sorter);

            // Sanity check to ensure the leader is *always* at the top of the party.
            if (leaderOfParty != null)
            {
                var index = toSort.FindIndex(character => character.Character.Equals(leaderOfParty));
                var leaderVm = toSort[index];
                toSort.RemoveAt(index);
                toSort.Insert(0, leaderVm);
            }

            // Since the only function we have to move characters in the roster is `ShiftTroopToIndex`, which probably moves characters
            // so that the one that is where we want to be, gets "knocked down", this means we could knock characters out of their intended position.
            // To solve this, we will iterate through the sorted list from the start, find the character in the original list, and move it to the bottom of the roster.
            foreach (var character in toSort)
            {
                // Find the index of the character in the sorted list.
                var index = toSort.IndexOf(character);
                if (index < 0) continue; // Character not found in the sorted list.

                // Move the character to the correct position in the roster.
                if (index != character.Index) rosterToSort.ShiftTroopToIndex(index, rosterToSort.Count - 1);
            }
        }
    }
}