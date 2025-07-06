using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;

namespace PartyScreenEnhancements.Widgets
{
    public class AscDescWidget : ButtonWidget
    {
        private Brush _downBrush;
        private bool _isDescending;
        private Brush _upBrush;

        public AscDescWidget(UIContext context) : base(context)
        {
        }

        [Editor]
        public Brush UpArrowBrush
        {
            get => _upBrush;
            set
            {
                if (_upBrush != value)
                {
                    _upBrush = value;
                    OnPropertyChanged(value);
                }
            }
        }

        [Editor]
        public Brush DownArrowBrush
        {
            get => _downBrush;
            set
            {
                if (_downBrush != value)
                {
                    _downBrush = value;
                    OnPropertyChanged(value);
                }
            }
        }

        [Editor]
        public bool IsDescending
        {
            get => _isDescending;
            set
            {
                if (_isDescending != value)
                {
                    _isDescending = value;
                    OnPropertyChanged(value);
                }

                UpdateVisual();
            }
        }

        private void UpdateVisual()
        {
            if (UpArrowBrush == null || DownArrowBrush == null) return;

            if (IsDescending)
                Brush = DownArrowBrush;
            else
                Brush = UpArrowBrush;
        }
    }
}