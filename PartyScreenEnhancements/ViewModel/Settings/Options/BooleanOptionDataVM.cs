using System;
using TaleWorlds.Library;

namespace PartyScreenEnhancements.ViewModel.Settings.Options
{
    /**
     * Almost an exact replica of the official BooleanOptionDataVM.
     * The fact that they use hard baked enums made it less of a chore to just roll my own OptionVM than use theirs.
     */
    public class BooleanOptionDataVM : GenericOptionDataVM
    {
        private readonly Action<bool> _setter;

        private bool _optionValue;

        public BooleanOptionDataVM(bool initialValue, string name, string description, Action<bool> setter) : base(name,
            description, 0)
        {
            _optionValue = initialValue;
            OptionValueAsBoolean = _optionValue;
            _setter = setter;
            ImageIDs = new[]
            {
                name + "_0",
                name + "_1"
            };
        }

        [DataSourceProperty]
        public bool OptionValueAsBoolean
        {
            get => _optionValue;
            set
            {
                if (value != _optionValue)
                {
                    _optionValue = value;
                    _setter(value);
                    OnPropertyChanged();
                }
            }
        }
    }
}