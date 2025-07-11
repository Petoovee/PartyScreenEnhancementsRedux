﻿using System;
using PartyScreenEnhancements.Saving;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace PartyScreenEnhancements.ViewModel
{
    public class RecruitPrisonerVM : TaleWorlds.Library.ViewModel
    {
        private MBBindingList<PartyCharacterVM> _mainPartyPrisoners;
        private PartyEnhancementsVM _parent;
        private PartyScreenLogic _partyLogic;
        private PartyVM _partyVM;

        private HintViewModel _recruitHint;

        public RecruitPrisonerVM(PartyEnhancementsVM parent, PartyVM partyVm, PartyScreenLogic logic)
        {
            _parent = parent;
            _partyVM = partyVm;
            _partyLogic = logic;
            _mainPartyPrisoners = _partyVM.MainPartyPrisoners;
            _recruitHint =
                new HintViewModel(
                    new TextObject("Recruit All Prisoners.\nClick with CTRL pressed to ignore party size limits"));
        }

        [DataSourceProperty]
        public HintViewModel RecruitHint
        {
            get => _recruitHint;
            set
            {
                if (value != _recruitHint)
                {
                    _recruitHint = value;
                    OnPropertyChanged();
                }
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            _mainPartyPrisoners = null;
            _partyLogic = null;
            _partyVM = null;
            _parent = null;
        }

        //TODO: Switch to cleaner RecruitByDefault=false logic.
        public void RecruitAll()
        {
            var shouldIgnoreLimit = Utilities.IsControlDown();
            var amountUpgraded = 0;

            try
            {
                var enumerator = new PartyCharacterVM[_mainPartyPrisoners.Count];
                _mainPartyPrisoners.CopyTo(enumerator, 0);

                foreach (var prisoner in enumerator)
                {
                    if (prisoner == null) continue;

                    var remainingPartySize = _partyLogic.RightOwnerParty.PartySizeLimit - _partyLogic
                        .MemberRosters[(int)PartyScreenLogic.PartyRosterSide.Right]
                        .TotalManCount;
                    if (remainingPartySize > 0 || shouldIgnoreLimit)
                    {
                        if (prisoner.IsTroopRecruitable)
                        {
                            _partyVM.CurrentCharacter = prisoner;

                            if (PartyScreenConfig.PrisonersToRecruit.TryGetValue(prisoner.Character.StringId,
                                    out var val))
                            {
                                if (val == -1 && PartyScreenConfig.ExtraSettings.RecruitByDefault)
                                    continue;
                            }
                            else if (!PartyScreenConfig.ExtraSettings.RecruitByDefault)
                            {
                                continue;
                            }

                            RecruitPrisoner(prisoner,
                                shouldIgnoreLimit ? prisoner.NumOfRecruitablePrisoners : remainingPartySize,
                                ref amountUpgraded);
                        }
                    }
                    else
                    {
                        if (PartyScreenConfig.ExtraSettings.ShowGeneralLogMessage)
                            InformationManager.DisplayMessage(
                                new InformationMessage($"Party size limit reached, {amountUpgraded} recruited!"));
                        return;
                    }
                }

                if (PartyScreenConfig.ExtraSettings.ShowGeneralLogMessage)
                    InformationManager.DisplayMessage(new InformationMessage($"Recruited {amountUpgraded} prisoners"));

                _parent.RefreshValues();
            }
            catch (Exception e)
            {
                Logging.Log(Logging.Levels.ERROR, $"Recruit Prisoners: {e}");
                Utilities.DisplayMessage($"PSE Recruit Prisoner Exception {e}");
            }
        }

        private void RecruitPrisoner(PartyCharacterVM character, int remainingSize, ref int amount)
        {
            if (!_partyLogic.IsPrisonerRecruitable(character.Type, character.Character, character.Side)) return;

            var number = Math.Min(character.NumOfRecruitablePrisoners, remainingSize);

            if (number > 0)
            {
                var partyCommand = new PartyScreenLogic.PartyCommand();
                partyCommand.FillForRecruitTroop(character.Side, character.Type,
                    character.Character, number, 0);

                _partyLogic.AddCommand(partyCommand);

                amount += number;
                character.UpdateRecruitable();
            }
        }
    }
}