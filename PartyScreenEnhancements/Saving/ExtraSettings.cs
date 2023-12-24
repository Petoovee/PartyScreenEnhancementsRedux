using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace PartyScreenEnhancements.Saving
{
    /// <summary>
    ///     Settings class used for any 'small' settings such as simple booleans
    ///     Instantiated and used primarily by <see cref="PartyScreenConfig" />
    /// </summary>
    public class ExtraSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _displayCategory;
        private bool _shouldShowCompletePartyNumber;

        [XmlElement("GeneralLog")] public bool ShowGeneralLogMessage { get; set; } = true;

        [XmlElement("RecruitByDefault")] public bool RecruitByDefault { get; set; } = true;
        [XmlElement("UpgradeOnDone")] public bool UpgradeOnDone { get; set; } = true;

        [XmlElement("CategoryNumbers")]
        public bool DisplayCategoryNumbers
        {
            get => _displayCategory;
            set
            {
                _displayCategory = value;
                OnPropertyChanged();
            }
        }

        [XmlElement("HalfHalfUpgrades")] public bool EqualUpgrades { get; set; }

        [XmlElement("ShouldShowCompletePartyNumber")]
        public bool ShouldShowCompletePartyNumber
        {
            get => _shouldShowCompletePartyNumber;
            set
            {
                _shouldShowCompletePartyNumber = value;
                OnPropertyChanged();
            }
        }

        [XmlElement("UpgradeTooltips")] public bool PathSelectTooltips { get; set; } = true;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}