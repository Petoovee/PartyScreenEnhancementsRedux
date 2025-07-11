﻿using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace PartyScreenEnhancements.ViewModel
{
    public class TransferWoundedTroopsVM : TaleWorlds.Library.ViewModel
    {
        private MBBindingList<PartyCharacterVM> _mainPartyList;
        private PartyEnhancementsVM _parent;

        private PartyVM _partyVm;
        private bool _shouldShowTransferWounded;


        private HintViewModel _woundedHint;

        public TransferWoundedTroopsVM(PartyEnhancementsVM parent, PartyVM partyVm, bool shouldShow)
        {
            _parent = parent;
            _partyVm = partyVm;
            _mainPartyList = partyVm?.MainPartyTroops;
            _shouldShowTransferWounded = shouldShow;
            _woundedHint = new HintViewModel(new TextObject("Transfer All Wounded"));
        }

        [DataSourceProperty]
        public HintViewModel WoundedHint
        {
            get => _woundedHint;
            set
            {
                if (value != _woundedHint)
                {
                    _woundedHint = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool ShouldShowTransferWounded
        {
            get => _shouldShowTransferWounded;
            set
            {
                if (value != _shouldShowTransferWounded)
                {
                    _shouldShowTransferWounded = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            _partyVm = null;
            _parent = null;
            _mainPartyList = null;
        }

        private void ExecuteTransferWounded()
        {
            try
            {
                var enumerator = new PartyCharacterVM[_mainPartyList.Count];
                _mainPartyList?.CopyTo(enumerator, 0);

                foreach (var character in enumerator)
                    if (character?.WoundedCount > 0)
                        if (character.IsTroopTransferrable)
                        {
                            var wounded = Math.Min(character.WoundedCount, character.Number);
                            PartyCharacterVM.OnTransfer(character, -1, wounded, character.Side);
                            character.InitializeUpgrades();
                        }

                _partyVm?.ExecuteRemoveZeroCounts();
                _parent.RefreshValues();
            }
            catch (Exception e)
            {
                Logging.Log(Logging.Levels.ERROR, $"Transfer Wounded Troops: {e}");
                Utilities.DisplayMessage($"PSE Transfer Wounded Exception: {e}");
            }
        }
    }
}