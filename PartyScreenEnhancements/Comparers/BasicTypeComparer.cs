﻿using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;

namespace PartyScreenEnhancements.Comparers
{
    public class BasicTypeComparer : PartySort
    {
        private Dictionary<string, Func<CharacterObject, CharacterObject, int>> _compDictionary;

        public BasicTypeComparer(PartySort equalSorter, bool descending, List<string> customSort = null) : base(
            equalSorter, descending, customSort)
        {
        }

        public BasicTypeComparer()
        {
        }

        public override string GetHintText()
        {
            return
                "Compares units based on their unit type (Infantry, Archers, Mounted).\nAscending order Horse Archers -> Cavalry -> Archers -> Infantry.\nDescending order is Infantry -> Archers -> Cavalry -> Horse Archers";
        }

        public override string GetName()
        {
            return "Unit Type Comparer";
        }

        public override bool HasCustomSettings()
        {
            return true;
        }

        protected override int localCompare(ref PartyCharacterVM x, ref PartyCharacterVM y)
        {
            if (_compDictionary == null) FillDictionary();

            var xChar = x.Character;
            var yChar = y.Character;

            if (xChar == null || yChar == null)
                return 1;

            var isXHorseArcher = xChar.IsRanged && xChar.IsMounted;
            var isYHorseArcher = yChar.IsRanged && yChar.IsMounted;

            if (isXHorseArcher || isYHorseArcher)
            {
                if (isXHorseArcher && isYHorseArcher)
                    return EqualSorter?.Compare(x, y) ?? 0;
            }
            else
            {
                if ((x.Character.IsInfantry && y.Character.IsInfantry) ||
                    (x.Character.IsMounted && y.Character.IsMounted) || (x.Character.IsRanged && y.Character.IsRanged))
                    return EqualSorter?.Compare(x, y) ?? 0;
            }

            foreach (var order in CustomSettingsList)
            {
                var function = _compDictionary[order];
                var value = function(xChar, yChar);

                if (value != int.MaxValue) return value;
            }

            return 1;
        }

        public override void FillCustomList()
        {
            base.FillCustomList();
            CustomSettingsList.AddRange(new[] { "Infantry", "Archers", "Cavalry", "Horse Archers" });
            if (_compDictionary == null) FillDictionary();
        }

        private void FillDictionary()
        {
            _compDictionary = new Dictionary<string, Func<CharacterObject, CharacterObject, int>>();
            _compDictionary.Add("Infantry", InfantryCompare);
            _compDictionary.Add("Archers", ArcherCompare);
            _compDictionary.Add("Cavalry", CavalryCompare);
            _compDictionary.Add("Horse Archers", HorseArcherCompare);
        }

        private int InfantryCompare(CharacterObject x, CharacterObject y)
        {
            if (Descending ? x.IsInfantry : y.IsInfantry) return -1;
            if (Descending ? y.IsInfantry : x.IsInfantry) return 1;

            // Need some sort of null value to indicate no match whatsoever.
            return int.MaxValue;
        }

        private int ArcherCompare(CharacterObject x, CharacterObject y)
        {
            if (Descending ? !x.IsMounted && x.IsRanged : !y.IsMounted && y.IsRanged) return -1;
            if (Descending ? !y.IsMounted && y.IsRanged : !x.IsMounted && x.IsRanged) return 1;

            // Need some sort of null value to indicate no match whatsoever.
            return int.MaxValue;
        }

        private int CavalryCompare(CharacterObject x, CharacterObject y)
        {
            if (Descending ? x.IsMounted && !x.IsRanged : y.IsMounted && !y.IsRanged) return -1;
            if (Descending ? y.IsMounted && !y.IsRanged : x.IsMounted && !x.IsRanged) return 1;

            // Need some sort of null value to indicate no match whatsoever.
            return int.MaxValue;
        }

        private int HorseArcherCompare(CharacterObject x, CharacterObject y)
        {
            if (Descending ? x.IsMounted && x.IsRanged : y.IsMounted && y.IsRanged) return -1;
            if (Descending ? y.IsMounted && y.IsRanged : x.IsMounted && x.IsRanged) return 1;

            // Need some sort of null value to indicate no match whatsoever.
            return int.MaxValue;
        }
    }
}