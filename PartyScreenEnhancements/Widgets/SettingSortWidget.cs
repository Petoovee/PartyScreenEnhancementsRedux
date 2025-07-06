using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace PartyScreenEnhancements.Widgets
{
    public class SettingSortWidget : ButtonWidget
    {
        private bool _hasCustomSettings;

        private Widget _main;
        private RichTextWidget _nameWidget;

        public SettingSortWidget(UIContext context) : base(context)
        {
            OverrideDefaultStateSwitchingEnabled = true;
            AddState("Selected");
        }

        [Editor()]
        public Widget Main
        {
            get => _main;
            set
            {
                if (_main != value)
                {
                    _main = value;
                    OnPropertyChanged(value);
                }
            }
        }

        public RichTextWidget NameWidget
        {
            get => _nameWidget;
            set
            {
                if (_nameWidget != value)
                {
                    _nameWidget = value;
                    OnPropertyChanged(value);
                }
            }
        }

        public bool HasCustomSettings
        {
            get => _hasCustomSettings;
            set
            {
                if (_hasCustomSettings != value)
                {
                    _hasCustomSettings = value;
                    OnPropertyChanged(value);
                }
            }
        }

        private void SetWidgetsState(string state)
        {
            base.SetState(state);
            _main.SetState(state);
        }

        protected override void OnLateUpdate(float dt)
        {
            base.OnLateUpdate(dt);
            if (HasCustomSettings) NameWidget.Brush.FontColor = Color.ConvertStringToColor("#FFD700FF");
        }

        protected override void RefreshState()
        {
            base.RefreshState();

            if (IsDisabled)
            {
                SetWidgetsState("Disabled");
                return;
            }

            if (IsPressed)
            {
                SetWidgetsState("Pressed");
                return;
            }

            if (IsHovered)
            {
                SetWidgetsState("Hovered");
                return;
            }

            if (IsSelected)
            {
                SetWidgetsState("Selected");
                return;
            }

            SetWidgetsState("Default");
        }

        public void ResetIsSelected()
        {
            IsSelected = false;
        }

        private void OnValueChanged(PropertyOwnerObject arg1, string arg2, object arg3)
        {
            if (arg2 == "ValueInt") AcceptDrag = (int)arg3 > 0;
        }
    }
}