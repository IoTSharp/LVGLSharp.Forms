using LVGLSharp.Darwing;
using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    internal enum ApplicationStyleMode
    {
        WinForms,
        Lvgl
    }

    internal sealed class ApplicationStyleSet
    {
        public ApplicationStyleSet(
            ButtonControlStyle button,
            ChoiceControlStyle checkBox,
            ChoiceControlStyle radioButton,
            TextControlStyle label,
            ListControlStyle listBox,
            ComboBoxControlStyle comboBox,
            TextInputControlStyle textBox,
            TextInputControlStyle richTextBox,
            ProgressBarControlStyle progressBar,
            TrackBarControlStyle trackBar,
            ContainerControlStyle root,
            ContainerControlStyle groupBox,
            ContainerControlStyle transparentPanel,
            ContainerControlStyle layoutPanel)
        {
            Button = button;
            CheckBox = checkBox;
            RadioButton = radioButton;
            Label = label;
            ListBox = listBox;
            ComboBox = comboBox;
            TextBox = textBox;
            RichTextBox = richTextBox;
            ProgressBar = progressBar;
            TrackBar = trackBar;
            Root = root;
            GroupBox = groupBox;
            TransparentPanel = transparentPanel;
            LayoutPanel = layoutPanel;
        }

        public ButtonControlStyle Button { get; }

        public ChoiceControlStyle CheckBox { get; }

        public ChoiceControlStyle RadioButton { get; }

        public TextControlStyle Label { get; }

        public ListControlStyle ListBox { get; }

        public ComboBoxControlStyle ComboBox { get; }

        public TextInputControlStyle TextBox { get; }

        public TextInputControlStyle RichTextBox { get; }

        public ProgressBarControlStyle ProgressBar { get; }

        public TrackBarControlStyle TrackBar { get; }

        public ContainerControlStyle Root { get; }

        public ContainerControlStyle GroupBox { get; }

        public ContainerControlStyle TransparentPanel { get; }

        public ContainerControlStyle LayoutPanel { get; }
    }

    internal readonly record struct TextControlStyle(
        bool UseThemeDefaults,
        Color TextColor,
        int LetterSpacing)
    {
        public unsafe void Apply(lv_obj_t* obj)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(TextColor), 0);
            lv_obj_set_style_text_letter_space(obj, LetterSpacing, 0);
        }
    }

    internal readonly record struct ChoiceControlStyle(
        bool UseThemeDefaults,
        Color TextColor,
        int IndicatorSize,
        int IndicatorRadius,
        int IndicatorBorderWidth,
        Color IndicatorBorderColor,
        Color IndicatorBackgroundColor,
        byte IndicatorBackgroundOpacity,
        Color CheckedIndicatorBorderColor,
        Color CheckedIndicatorBackgroundColor,
        byte CheckedIndicatorBackgroundOpacity,
        Color CheckedMarkColor,
        int TextGap,
        int OutlineWidth,
        Color OutlineColor,
        uint AnimationDuration)
    {
        public unsafe void Apply(lv_obj_t* obj)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            var indicatorSelector = LvglStyleSelector.ForPart(LV_PART_INDICATOR);
            var checkedIndicatorSelector = LvglStyleSelector.ForPartAndState(LV_PART_INDICATOR, LV_STATE_CHECKED);

            lv_obj_set_style_bg_opa(obj, (byte)LV_OPA_TRANSP, 0);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(TextColor), 0);
            lv_obj_set_style_pad_column(obj, TextGap, 0);
            lv_obj_set_style_anim_duration(obj, AnimationDuration, 0);

            lv_obj_set_style_width(obj, IndicatorSize, indicatorSelector);
            lv_obj_set_style_height(obj, IndicatorSize, indicatorSelector);
            lv_obj_set_style_radius(obj, IndicatorRadius, indicatorSelector);
            lv_obj_set_style_border_width(obj, IndicatorBorderWidth, indicatorSelector);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(IndicatorBorderColor), indicatorSelector);
            lv_obj_set_style_bg_opa(obj, IndicatorBackgroundOpacity, indicatorSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(IndicatorBackgroundColor), indicatorSelector);
            lv_obj_set_style_anim_duration(obj, AnimationDuration, indicatorSelector);

            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(CheckedIndicatorBorderColor), checkedIndicatorSelector);
            lv_obj_set_style_bg_opa(obj, CheckedIndicatorBackgroundOpacity, checkedIndicatorSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(CheckedIndicatorBackgroundColor), checkedIndicatorSelector);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(CheckedMarkColor), checkedIndicatorSelector);
            lv_obj_set_style_outline_width(obj, OutlineWidth, checkedIndicatorSelector);
            lv_obj_set_style_outline_color(obj, LvglStyleColorConverter.ToLvColor(OutlineColor), checkedIndicatorSelector);
            lv_obj_set_style_outline_pad(obj, 0, checkedIndicatorSelector);
            lv_obj_set_style_anim_duration(obj, AnimationDuration, checkedIndicatorSelector);
        }
    }

    internal readonly record struct ListControlStyle(
        bool UseThemeDefaults,
        Color BackgroundColor,
        byte BackgroundOpacity,
        Color BorderColor,
        int BorderWidth,
        int Radius,
        int HorizontalPadding,
        int VerticalPadding,
        int ItemSpacing,
        Color TextColor,
        Color ItemBackgroundColor,
        byte ItemBackgroundOpacity,
        Color ItemBorderColor,
        int ItemBorderWidth,
        int ItemRadius)
    {
        public unsafe void Apply(lv_obj_t* obj)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            var itemSelector = LvglStyleSelector.ForPart(LV_PART_ITEMS);

            lv_obj_set_style_bg_opa(obj, BackgroundOpacity, 0);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(BackgroundColor), 0);
            lv_obj_set_style_border_width(obj, BorderWidth, 0);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(BorderColor), 0);
            lv_obj_set_style_radius(obj, Radius, 0);
            lv_obj_set_style_pad_hor(obj, HorizontalPadding, 0);
            lv_obj_set_style_pad_ver(obj, VerticalPadding, 0);
            lv_obj_set_style_pad_row(obj, ItemSpacing, 0);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(TextColor), 0);

            lv_obj_set_style_bg_opa(obj, ItemBackgroundOpacity, itemSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(ItemBackgroundColor), itemSelector);
            lv_obj_set_style_border_width(obj, ItemBorderWidth, itemSelector);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(ItemBorderColor), itemSelector);
            lv_obj_set_style_radius(obj, ItemRadius, itemSelector);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(TextColor), itemSelector);
        }
    }

    internal readonly record struct ComboBoxControlStyle(
        bool UseThemeDefaults,
        Color BackgroundColor,
        byte BackgroundOpacity,
        Color BorderColor,
        int BorderWidth,
        int Radius,
        int HorizontalPadding,
        int VerticalPadding,
        Color TextColor,
        uint AnimationDuration,
        Color ListBackgroundColor,
        byte ListBackgroundOpacity,
        Color ListBorderColor,
        int ListBorderWidth,
        int ListRadius,
        Color ListItemTextColor,
        Color SelectedItemBackgroundColor,
        byte SelectedItemBackgroundOpacity,
        Color SelectedItemTextColor)
    {
        public unsafe void Apply(lv_obj_t* obj, lv_obj_t* list)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            var indicatorSelector = LvglStyleSelector.ForPart(LV_PART_INDICATOR);
            var itemSelector = LvglStyleSelector.ForPart(LV_PART_ITEMS);
            var selectedSelector = LvglStyleSelector.ForPart(LV_PART_SELECTED);

            lv_obj_set_style_bg_opa(obj, BackgroundOpacity, 0);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(BackgroundColor), 0);
            lv_obj_set_style_border_width(obj, BorderWidth, 0);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(BorderColor), 0);
            lv_obj_set_style_radius(obj, Radius, 0);
            lv_obj_set_style_pad_hor(obj, HorizontalPadding, 0);
            lv_obj_set_style_pad_ver(obj, VerticalPadding, 0);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(TextColor), 0);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(TextColor), indicatorSelector);
            lv_obj_set_style_anim_duration(obj, AnimationDuration, 0);

            if (list == null)
            {
                return;
            }

            lv_obj_set_style_bg_opa(list, ListBackgroundOpacity, 0);
            lv_obj_set_style_bg_color(list, LvglStyleColorConverter.ToLvColor(ListBackgroundColor), 0);
            lv_obj_set_style_border_width(list, ListBorderWidth, 0);
            lv_obj_set_style_border_color(list, LvglStyleColorConverter.ToLvColor(ListBorderColor), 0);
            lv_obj_set_style_radius(list, ListRadius, 0);
            lv_obj_set_style_text_color(list, LvglStyleColorConverter.ToLvColor(ListItemTextColor), itemSelector);
            lv_obj_set_style_bg_opa(list, SelectedItemBackgroundOpacity, selectedSelector);
            lv_obj_set_style_bg_color(list, LvglStyleColorConverter.ToLvColor(SelectedItemBackgroundColor), selectedSelector);
            lv_obj_set_style_text_color(list, LvglStyleColorConverter.ToLvColor(SelectedItemTextColor), selectedSelector);
            lv_obj_set_style_anim_duration(list, AnimationDuration, 0);
            lv_obj_set_style_anim_duration(list, AnimationDuration, itemSelector);
            lv_obj_set_style_anim_duration(list, AnimationDuration, selectedSelector);
        }
    }

    internal readonly record struct TextInputControlStyle(
        bool UseThemeDefaults,
        Color BackgroundColor,
        byte BackgroundOpacity,
        Color BorderColor,
        int BorderWidth,
        int Radius,
        int HorizontalPadding,
        int VerticalPadding,
        Color TextColor,
        Color CursorColor,
        int CursorWidth,
        Color SelectionBackgroundColor,
        byte SelectionBackgroundOpacity,
        Color SelectionTextColor)
    {
        public unsafe void Apply(lv_obj_t* obj)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            var cursorSelector = LvglStyleSelector.ForPart(LV_PART_CURSOR);
            var selectedSelector = LvglStyleSelector.ForPart(LV_PART_SELECTED);

            lv_obj_set_style_bg_opa(obj, BackgroundOpacity, 0);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(BackgroundColor), 0);
            lv_obj_set_style_border_width(obj, BorderWidth, 0);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(BorderColor), 0);
            lv_obj_set_style_radius(obj, Radius, 0);
            lv_obj_set_style_pad_hor(obj, HorizontalPadding, 0);
            lv_obj_set_style_pad_ver(obj, VerticalPadding, 0);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(TextColor), 0);

            lv_obj_set_style_bg_opa(obj, (byte)LV_OPA_COVER, cursorSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(CursorColor), cursorSelector);
            lv_obj_set_style_width(obj, CursorWidth, cursorSelector);

            lv_obj_set_style_bg_opa(obj, SelectionBackgroundOpacity, selectedSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(SelectionBackgroundColor), selectedSelector);
            lv_obj_set_style_text_color(obj, LvglStyleColorConverter.ToLvColor(SelectionTextColor), selectedSelector);
        }
    }

    internal readonly record struct ProgressBarControlStyle(
        bool UseThemeDefaults,
        Color BackgroundColor,
        byte BackgroundOpacity,
        Color BorderColor,
        int BorderWidth,
        int Radius,
        Color IndicatorBackgroundColor,
        byte IndicatorBackgroundOpacity)
    {
        public unsafe void Apply(lv_obj_t* obj)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            var indicatorSelector = LvglStyleSelector.ForPart(LV_PART_INDICATOR);

            lv_obj_set_style_bg_opa(obj, BackgroundOpacity, 0);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(BackgroundColor), 0);
            lv_obj_set_style_border_width(obj, BorderWidth, 0);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(BorderColor), 0);
            lv_obj_set_style_radius(obj, Radius, 0);
            lv_obj_set_style_bg_opa(obj, IndicatorBackgroundOpacity, indicatorSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(IndicatorBackgroundColor), indicatorSelector);
            lv_obj_set_style_radius(obj, Radius, indicatorSelector);
        }
    }

    internal readonly record struct TrackBarControlStyle(
        bool UseThemeDefaults,
        Color BackgroundColor,
        byte BackgroundOpacity,
        Color BorderColor,
        int BorderWidth,
        int Radius,
        Color IndicatorBackgroundColor,
        byte IndicatorBackgroundOpacity,
        Color KnobBackgroundColor,
        byte KnobBackgroundOpacity,
        Color KnobBorderColor,
        int KnobBorderWidth,
        int KnobSize,
        int KnobRadius)
    {
        public unsafe void Apply(lv_obj_t* obj)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            var indicatorSelector = LvglStyleSelector.ForPart(LV_PART_INDICATOR);
            var knobSelector = LvglStyleSelector.ForPart(LV_PART_KNOB);

            lv_obj_set_style_bg_opa(obj, BackgroundOpacity, 0);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(BackgroundColor), 0);
            lv_obj_set_style_border_width(obj, BorderWidth, 0);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(BorderColor), 0);
            lv_obj_set_style_radius(obj, Radius, 0);

            lv_obj_set_style_bg_opa(obj, IndicatorBackgroundOpacity, indicatorSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(IndicatorBackgroundColor), indicatorSelector);
            lv_obj_set_style_radius(obj, Radius, indicatorSelector);

            lv_obj_set_style_bg_opa(obj, KnobBackgroundOpacity, knobSelector);
            lv_obj_set_style_bg_color(obj, LvglStyleColorConverter.ToLvColor(KnobBackgroundColor), knobSelector);
            lv_obj_set_style_border_width(obj, KnobBorderWidth, knobSelector);
            lv_obj_set_style_border_color(obj, LvglStyleColorConverter.ToLvColor(KnobBorderColor), knobSelector);
            lv_obj_set_style_width(obj, KnobSize, knobSelector);
            lv_obj_set_style_height(obj, KnobSize, knobSelector);
            lv_obj_set_style_radius(obj, KnobRadius, knobSelector);
        }
    }

    internal readonly record struct ButtonControlStyle(
        bool UseThemeDefaults,
        Color BackgroundColor,
        byte BackgroundOpacity,
        Color BorderColor,
        int BorderWidth,
        int Radius,
        int HorizontalPadding,
        int VerticalPadding,
        Color TextColor,
        uint AnimationDuration)
    {
        public unsafe void Apply(lv_obj_t* button, lv_obj_t* label)
        {
            if (UseThemeDefaults)
            {
                return;
            }

            lv_obj_set_style_bg_opa(button, BackgroundOpacity, 0);
            lv_obj_set_style_bg_color(button, LvglStyleColorConverter.ToLvColor(BackgroundColor), 0);
            lv_obj_set_style_border_width(button, BorderWidth, 0);
            lv_obj_set_style_border_color(button, LvglStyleColorConverter.ToLvColor(BorderColor), 0);
            lv_obj_set_style_radius(button, Radius, 0);
            lv_obj_set_style_pad_hor(button, HorizontalPadding, 0);
            lv_obj_set_style_pad_ver(button, VerticalPadding, 0);
            lv_obj_set_style_text_color(label, LvglStyleColorConverter.ToLvColor(TextColor), 0);
            lv_obj_set_style_anim_duration(button, AnimationDuration, 0);
        }
    }

    internal readonly record struct ContainerControlStyle(
        int? PaddingAll = null,
        int? BorderWidth = null,
        byte? BackgroundOpacity = null)
    {
        public unsafe void Apply(lv_obj_t* obj)
        {
            if (PaddingAll.HasValue)
            {
                lv_obj_set_style_pad_all(obj, PaddingAll.Value, 0);
            }

            if (BorderWidth.HasValue)
            {
                lv_obj_set_style_border_width(obj, BorderWidth.Value, 0);
            }

            if (BackgroundOpacity.HasValue)
            {
                lv_obj_set_style_bg_opa(obj, BackgroundOpacity.Value, 0);
            }
        }
    }

    internal static class ApplicationStyleCatalog
    {
        private static readonly ApplicationStyleSet s_winForms = new(
            button: new ButtonControlStyle(
                UseThemeDefaults: false,
                BackgroundColor: new Color(240, 240, 240),
                BackgroundOpacity: (byte)LV_OPA_COVER,
                BorderColor: new Color(173, 173, 173),
                BorderWidth: 1,
                Radius: 4,
                HorizontalPadding: 12,
                VerticalPadding: 6,
                TextColor: new Color(0, 0, 0),
                AnimationDuration: 0),
            checkBox: new ChoiceControlStyle(
                UseThemeDefaults: false,
                TextColor: new Color(0, 0, 0),
                IndicatorSize: 16,
                IndicatorRadius: 3,
                IndicatorBorderWidth: 1,
                IndicatorBorderColor: new Color(122, 122, 122),
                IndicatorBackgroundColor: new Color(255, 255, 255),
                IndicatorBackgroundOpacity: (byte)LV_OPA_COVER,
                CheckedIndicatorBorderColor: new Color(0, 120, 215),
                CheckedIndicatorBackgroundColor: new Color(0, 120, 215),
                CheckedIndicatorBackgroundOpacity: (byte)LV_OPA_COVER,
                CheckedMarkColor: new Color(255, 255, 255),
                TextGap: 8,
                OutlineWidth: 1,
                OutlineColor: new Color(153, 209, 255),
                AnimationDuration: 0),
            radioButton: new ChoiceControlStyle(
                UseThemeDefaults: false,
                TextColor: new Color(0, 0, 0),
                IndicatorSize: 16,
                IndicatorRadius: LV_RADIUS_CIRCLE,
                IndicatorBorderWidth: 1,
                IndicatorBorderColor: new Color(122, 122, 122),
                IndicatorBackgroundColor: new Color(255, 255, 255),
                IndicatorBackgroundOpacity: (byte)LV_OPA_COVER,
                CheckedIndicatorBorderColor: new Color(0, 120, 215),
                CheckedIndicatorBackgroundColor: new Color(0, 120, 215),
                CheckedIndicatorBackgroundOpacity: (byte)LV_OPA_COVER,
                CheckedMarkColor: new Color(0, 120, 215),
                TextGap: 8,
                OutlineWidth: 1,
                OutlineColor: new Color(153, 209, 255),
                AnimationDuration: 0),
            label: new TextControlStyle(
                UseThemeDefaults: false,
                TextColor: new Color(0, 0, 0),
                LetterSpacing: 0),
            listBox: new ListControlStyle(
                UseThemeDefaults: false,
                BackgroundColor: new Color(255, 255, 255),
                BackgroundOpacity: (byte)LV_OPA_COVER,
                BorderColor: new Color(122, 122, 122),
                BorderWidth: 1,
                Radius: 2,
                HorizontalPadding: 2,
                VerticalPadding: 2,
                ItemSpacing: 1,
                TextColor: new Color(0, 0, 0),
                ItemBackgroundColor: new Color(255, 255, 255),
                ItemBackgroundOpacity: (byte)LV_OPA_TRANSP,
                ItemBorderColor: new Color(255, 255, 255),
                ItemBorderWidth: 0,
                ItemRadius: 0),
            comboBox: new ComboBoxControlStyle(
                UseThemeDefaults: false,
                BackgroundColor: new Color(255, 255, 255),
                BackgroundOpacity: (byte)LV_OPA_COVER,
                BorderColor: new Color(122, 122, 122),
                BorderWidth: 1,
                Radius: 2,
                HorizontalPadding: 8,
                VerticalPadding: 4,
                TextColor: new Color(0, 0, 0),
                AnimationDuration: 0,
                ListBackgroundColor: new Color(255, 255, 255),
                ListBackgroundOpacity: (byte)LV_OPA_COVER,
                ListBorderColor: new Color(122, 122, 122),
                ListBorderWidth: 1,
                ListRadius: 2,
                ListItemTextColor: new Color(0, 0, 0),
                SelectedItemBackgroundColor: new Color(0, 120, 215),
                SelectedItemBackgroundOpacity: (byte)LV_OPA_COVER,
                SelectedItemTextColor: new Color(255, 255, 255)),
            textBox: new TextInputControlStyle(
                UseThemeDefaults: false,
                BackgroundColor: new Color(255, 255, 255),
                BackgroundOpacity: (byte)LV_OPA_COVER,
                BorderColor: new Color(122, 122, 122),
                BorderWidth: 1,
                Radius: 2,
                HorizontalPadding: 6,
                VerticalPadding: 4,
                TextColor: new Color(0, 0, 0),
                CursorColor: new Color(0, 120, 215),
                CursorWidth: 1,
                SelectionBackgroundColor: new Color(0, 120, 215),
                SelectionBackgroundOpacity: (byte)LV_OPA_COVER,
                SelectionTextColor: new Color(255, 255, 255)),
            richTextBox: new TextInputControlStyle(
                UseThemeDefaults: false,
                BackgroundColor: new Color(255, 255, 255),
                BackgroundOpacity: (byte)LV_OPA_COVER,
                BorderColor: new Color(122, 122, 122),
                BorderWidth: 1,
                Radius: 2,
                HorizontalPadding: 6,
                VerticalPadding: 6,
                TextColor: new Color(0, 0, 0),
                CursorColor: new Color(0, 120, 215),
                CursorWidth: 1,
                SelectionBackgroundColor: new Color(0, 120, 215),
                SelectionBackgroundOpacity: (byte)LV_OPA_COVER,
                SelectionTextColor: new Color(255, 255, 255)),
            progressBar: new ProgressBarControlStyle(
                UseThemeDefaults: false,
                BackgroundColor: new Color(230, 230, 230),
                BackgroundOpacity: (byte)LV_OPA_COVER,
                BorderColor: new Color(173, 173, 173),
                BorderWidth: 1,
                Radius: 2,
                IndicatorBackgroundColor: new Color(0, 120, 215),
                IndicatorBackgroundOpacity: (byte)LV_OPA_COVER),
            trackBar: new TrackBarControlStyle(
                UseThemeDefaults: false,
                BackgroundColor: new Color(214, 214, 214),
                BackgroundOpacity: (byte)LV_OPA_COVER,
                BorderColor: new Color(214, 214, 214),
                BorderWidth: 0,
                Radius: LV_RADIUS_CIRCLE,
                IndicatorBackgroundColor: new Color(0, 120, 215),
                IndicatorBackgroundOpacity: (byte)LV_OPA_COVER,
                KnobBackgroundColor: new Color(255, 255, 255),
                KnobBackgroundOpacity: (byte)LV_OPA_COVER,
                KnobBorderColor: new Color(122, 122, 122),
                KnobBorderWidth: 1,
                KnobSize: 16,
                KnobRadius: LV_RADIUS_CIRCLE),
            root: new ContainerControlStyle(PaddingAll: 0),
            groupBox: new ContainerControlStyle(PaddingAll: 4),
            transparentPanel: new ContainerControlStyle(PaddingAll: 0, BorderWidth: 0, BackgroundOpacity: (byte)LV_OPA_TRANSP),
            layoutPanel: new ContainerControlStyle(PaddingAll: 0));

        private static readonly ApplicationStyleSet s_lvgl = new(
            button: new ButtonControlStyle(
                UseThemeDefaults: true,
                BackgroundColor: default,
                BackgroundOpacity: default,
                BorderColor: default,
                BorderWidth: default,
                Radius: default,
                HorizontalPadding: default,
                VerticalPadding: default,
                TextColor: default,
                AnimationDuration: default),
            checkBox: new ChoiceControlStyle(
                UseThemeDefaults: true,
                TextColor: default,
                IndicatorSize: default,
                IndicatorRadius: default,
                IndicatorBorderWidth: default,
                IndicatorBorderColor: default,
                IndicatorBackgroundColor: default,
                IndicatorBackgroundOpacity: default,
                CheckedIndicatorBorderColor: default,
                CheckedIndicatorBackgroundColor: default,
                CheckedIndicatorBackgroundOpacity: default,
                CheckedMarkColor: default,
                TextGap: default,
                OutlineWidth: default,
                OutlineColor: default,
                AnimationDuration: default),
            radioButton: new ChoiceControlStyle(
                UseThemeDefaults: true,
                TextColor: default,
                IndicatorSize: default,
                IndicatorRadius: default,
                IndicatorBorderWidth: default,
                IndicatorBorderColor: default,
                IndicatorBackgroundColor: default,
                IndicatorBackgroundOpacity: default,
                CheckedIndicatorBorderColor: default,
                CheckedIndicatorBackgroundColor: default,
                CheckedIndicatorBackgroundOpacity: default,
                CheckedMarkColor: default,
                TextGap: default,
                OutlineWidth: default,
                OutlineColor: default,
                AnimationDuration: default),
            label: new TextControlStyle(
                UseThemeDefaults: true,
                TextColor: default,
                LetterSpacing: default),
            listBox: new ListControlStyle(
                UseThemeDefaults: true,
                BackgroundColor: default,
                BackgroundOpacity: default,
                BorderColor: default,
                BorderWidth: default,
                Radius: default,
                HorizontalPadding: default,
                VerticalPadding: default,
                ItemSpacing: default,
                TextColor: default,
                ItemBackgroundColor: default,
                ItemBackgroundOpacity: default,
                ItemBorderColor: default,
                ItemBorderWidth: default,
                ItemRadius: default),
            comboBox: new ComboBoxControlStyle(
                UseThemeDefaults: true,
                BackgroundColor: default,
                BackgroundOpacity: default,
                BorderColor: default,
                BorderWidth: default,
                Radius: default,
                HorizontalPadding: default,
                VerticalPadding: default,
                TextColor: default,
                AnimationDuration: default,
                ListBackgroundColor: default,
                ListBackgroundOpacity: default,
                ListBorderColor: default,
                ListBorderWidth: default,
                ListRadius: default,
                ListItemTextColor: default,
                SelectedItemBackgroundColor: default,
                SelectedItemBackgroundOpacity: default,
                SelectedItemTextColor: default),
            textBox: new TextInputControlStyle(
                UseThemeDefaults: true,
                BackgroundColor: default,
                BackgroundOpacity: default,
                BorderColor: default,
                BorderWidth: default,
                Radius: default,
                HorizontalPadding: default,
                VerticalPadding: default,
                TextColor: default,
                CursorColor: default,
                CursorWidth: default,
                SelectionBackgroundColor: default,
                SelectionBackgroundOpacity: default,
                SelectionTextColor: default),
            richTextBox: new TextInputControlStyle(
                UseThemeDefaults: true,
                BackgroundColor: default,
                BackgroundOpacity: default,
                BorderColor: default,
                BorderWidth: default,
                Radius: default,
                HorizontalPadding: default,
                VerticalPadding: default,
                TextColor: default,
                CursorColor: default,
                CursorWidth: default,
                SelectionBackgroundColor: default,
                SelectionBackgroundOpacity: default,
                SelectionTextColor: default),
            progressBar: new ProgressBarControlStyle(
                UseThemeDefaults: true,
                BackgroundColor: default,
                BackgroundOpacity: default,
                BorderColor: default,
                BorderWidth: default,
                Radius: default,
                IndicatorBackgroundColor: default,
                IndicatorBackgroundOpacity: default),
            trackBar: new TrackBarControlStyle(
                UseThemeDefaults: true,
                BackgroundColor: default,
                BackgroundOpacity: default,
                BorderColor: default,
                BorderWidth: default,
                Radius: default,
                IndicatorBackgroundColor: default,
                IndicatorBackgroundOpacity: default,
                KnobBackgroundColor: default,
                KnobBackgroundOpacity: default,
                KnobBorderColor: default,
                KnobBorderWidth: default,
                KnobSize: default,
                KnobRadius: default),
            root: new ContainerControlStyle(PaddingAll: 0),
            groupBox: new ContainerControlStyle(PaddingAll: 4),
            transparentPanel: new ContainerControlStyle(PaddingAll: 0, BorderWidth: 0, BackgroundOpacity: (byte)LV_OPA_TRANSP),
            layoutPanel: new ContainerControlStyle(PaddingAll: 0));

        public static ApplicationStyleSet Get(ApplicationStyleMode mode)
        {
            return mode == ApplicationStyleMode.WinForms ? s_winForms : s_lvgl;
        }
    }

    internal static class LvglStyleColorConverter
    {
        public static lv_color_t ToLvColor(Color color)
        {
            return lv_color_make(color.R, color.G, color.B);
        }
    }

    internal static class LvglStyleSelector
    {
        public static uint ForPart(int part)
        {
            return (uint)part;
        }

        public static uint ForPartAndState(int part, int state)
        {
            return (uint)(part | state);
        }
    }
}
