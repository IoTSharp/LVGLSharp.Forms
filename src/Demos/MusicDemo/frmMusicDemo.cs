using LVGLSharp;
using LVGLSharp.Interop;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static LVGLSharp.Interop.LVGL;

using ImageSharpImage = SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>;

namespace MusicDemo;

public unsafe sealed class frmMusicDemo : Form
{
    private const int DemoWidth = 478;
    private const int DemoHeight = 271;
    private const int PlayerHeight = 238;
    private const int CoverSize = 128;
    private const int SpectrumBarCount = 24;
    private const uint LvImageHeaderMagic = 0x19;

    private const int CommandToggleList = 1;
    private const int CommandHideList = 2;
    private const int CommandCloseList = 3;
    private const int CommandTogglePlay = 4;
    private const int CommandPrevTrack = 5;
    private const int CommandNextTrack = 6;
    private const int CommandToggleShuffle = 7;
    private const int CommandToggleLoop = 8;
    private const int CommandSeekSlider = 9;
    private const int CommandTrackBase = 100;

    private static readonly TrackInfo[] s_tracks =
    [
        new("Waiting for true love", "The John Smith Band", "Rock - 1997", 74, 0x7B6AF6, 0xF18CB8, 0x6F8AF6, 11),
        new("Need a Better Future", "My True Name", "Drum'n bass - 2016", 146, 0xFF8F70, 0xFF5E7D, 0xFFB26A, 29),
        new("Vibrations", "Robotics", "Psy trance - 2020", 114, 0x00C2C7, 0x4E83FF, 0x72E3D7, 47),
        new("Why now?", "John Smith", "Metal - 2015", 144, 0x7B6AF6, 0xF18CB8, 0x6F8AF6, 59),
        new("Never Look Back", "My True Name", "Metal - 2015", 157, 0xFF8F70, 0xFF5E7D, 0xFFB26A, 73),
        new("It happened Yesterday", "Robotics", "Metal - 2015", 213, 0x00C2C7, 0x4E83FF, 0x72E3D7, 83),
        new("Feeling so High", "Robotics", "Metal - 2015", 116, 0x7B6AF6, 0xF18CB8, 0x6F8AF6, 97),
        new("Go Deeper", "Unknown artist", "Metal - 2015", 211, 0xFF8F70, 0xFF5E7D, 0xFFB26A, 109),
        new("Find You There", "Unknown artist", "Metal - 2015", 140, 0x00C2C7, 0x4E83FF, 0x72E3D7, 127),
        new("Until the End", "Unknown artist", "Metal - 2015", 139, 0x7B6AF6, 0xF18CB8, 0x6F8AF6, 149),
    ];

    private static frmMusicDemo? s_activeDemo;

    private readonly List<SixLaborsFontManager> _fontManagers = new();
    private readonly SpectrumBar[] _spectrumBars = new SpectrumBar[SpectrumBarCount];
    private readonly TrackRow[] _trackRows = new TrackRow[s_tracks.Length];

    private ImageDescriptor[] _coverDescriptors = Array.Empty<ImageDescriptor>();

    private lv_obj_t* _root;
    private lv_obj_t* _player;
    private lv_obj_t* _titleLabel;
    private lv_obj_t* _artistLabel;
    private lv_obj_t* _genreLabel;
    private lv_obj_t* _progressSlider;
    private lv_obj_t* _elapsedLabel;
    private lv_obj_t* _durationLabel;
    private lv_obj_t* _shuffleButton;
    private lv_obj_t* _shuffleIcon;
    private lv_obj_t* _loopButton;
    private lv_obj_t* _loopIcon;
    private lv_obj_t* _prevButton;
    private lv_obj_t* _prevIcon;
    private lv_obj_t* _playButton;
    private lv_obj_t* _playIcon;
    private lv_obj_t* _nextButton;
    private lv_obj_t* _nextIcon;
    private lv_obj_t* _albumGlow;
    private lv_obj_t* _albumRing;
    private lv_obj_t* _albumClip;
    private lv_obj_t* _albumImage;
    private lv_obj_t* _topBlob;
    private lv_obj_t* _bottomBlobLeft;
    private lv_obj_t* _bottomBlobRight;
    private lv_obj_t* _footerButton;
    private lv_obj_t* _footerLabel;
    private lv_obj_t* _footerCountLabel;
    private lv_obj_t* _trackSheetMask;
    private lv_obj_t* _trackSheet;
    private lv_obj_t* _trackList;

    private lv_timer_t* _uiTimer;
    private lv_font_t* _fallbackFont;
    private SixLaborsFontManager? _titleFont;
    private SixLaborsFontManager? _bodyFont;
    private SixLaborsFontManager? _smallFont;
    private SixLaborsFontManager? _iconFont;

    private long _lastTickMs;
    private int _currentTrackIndex;
    private int _playbackMilliseconds;
    private bool _playing;
    private bool _shuffleEnabled;
    private bool _loopEnabled = true;
    private bool _ignoreSliderEvents;
    private bool _trackSheetVisible;

    public frmMusicDemo()
    {
        Text = "LVGL Music Demo";
        ClientSize = new LVGLSharp.Drawing.Size(DemoWidth, DemoHeight);
        FormBorderStyle = FormBorderStyle.None;
        Load += OnMusicDemoLoad;
    }

    protected override void DestroyHandle()
    {
        CleanupResources();
        base.DestroyHandle();
    }

    private void OnMusicDemoLoad(object? sender, EventArgs e)
    {
        s_activeDemo = this;
        _root = (lv_obj_t*)Handle;
        _fallbackFont = lv_obj_get_style_text_font(_root, LV_PART_MAIN);

        ConfigureRoot();
        CreateFonts();
        GenerateCoverDescriptors();
        CreateScene();
        LoadTrack(0, startPlayback: false, resetProgress: true, animateCover: false);
        StartUiTimer();
    }

    private void ConfigureRoot()
    {
        lv_obj_set_style_bg_color(_root, lv_color_hex(0x343247), 0);
        lv_obj_set_style_bg_opa(_root, LV_OPA_COVER, 0);
        lv_obj_set_style_border_width(_root, 0, 0);
        lv_obj_set_style_radius(_root, 0, 0);
        lv_obj_set_style_pad_all(_root, 0, 0);
        lv_obj_clear_flag(_root,
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE |
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_ELASTIC |
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_MOMENTUM |
            lv_obj_flag_t.LV_OBJ_FLAG_SCROLL_CHAIN);
        lv_obj_set_scrollbar_mode(_root, lv_scrollbar_mode_t.LV_SCROLLBAR_MODE_OFF);
    }

    private void CreateFonts()
    {
        FontFamily? family = ResolveFontFamily();
        if (family is null || _fallbackFont == null)
        {
            return;
        }

        _titleFont = RegisterFontManager(new SixLaborsFontManager(family!, 20, 96f, _fallbackFont));
        _bodyFont = RegisterFontManager(new SixLaborsFontManager(family!, 13, 96f, _fallbackFont));
        _smallFont = RegisterFontManager(new SixLaborsFontManager(family!, 11, 96f, _fallbackFont));
        _iconFont = RegisterFontManager(new SixLaborsFontManager(family!, 16, 96f, _fallbackFont));
    }

    private void GenerateCoverDescriptors()
    {
        DisposeCoverDescriptors();
        _coverDescriptors = new ImageDescriptor[s_tracks.Length];

        for (int index = 0; index < s_tracks.Length; index++)
        {
            using ImageSharpImage image = new(CoverSize, CoverSize);
            PaintCoverImage(image, s_tracks[index]);
            _coverDescriptors[index] = CreateImageDescriptor(image);
        }
    }

    private void CreateScene()
    {
        CreatePlayerSurface();
        CreateHeader();
        CreateControlStrip();
        CreateAlbumArea();
        CreateFooter();
        CreateTrackSheet();
    }

    private void CreatePlayerSurface()
    {
        _player = lv_obj_create(_root);
        lv_obj_set_pos(_player, 0, 0);
        lv_obj_set_size(_player, DemoWidth, PlayerHeight);
        lv_obj_set_style_bg_color(_player, lv_color_hex(0xFFFEFF), 0);
        lv_obj_set_style_bg_grad_color(_player, lv_color_hex(0xF7F3FC), 0);
        lv_obj_set_style_bg_grad_dir(_player, lv_grad_dir_t.LV_GRAD_DIR_VER, 0);
        lv_obj_set_style_border_width(_player, 0, 0);
        lv_obj_set_style_pad_all(_player, 0, 0);
        lv_obj_set_style_radius(_player, 0, 0);
        lv_obj_clear_flag(_player, lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE);
        lv_obj_set_scrollbar_mode(_player, lv_scrollbar_mode_t.LV_SCROLLBAR_MODE_OFF);

        _topBlob = CreateBlob(_player, -24, -34, 238, 126, 0xF6EEFC, 255);
        _bottomBlobLeft = CreateBlob(_player, -38, 176, 210, 78, 0xF7F0FD, 255);
        _bottomBlobRight = CreateBlob(_player, 248, 160, 240, 100, 0xF4ECFB, 255);
    }

    private void CreateHeader()
    {
        _titleLabel = lv_label_create(_player);
        lv_obj_set_pos(_titleLabel, 30, 30);
        lv_obj_set_width(_titleLabel, 210);
        lv_label_set_long_mode(_titleLabel, LV_LABEL_LONG_SCROLL_CIRCULAR);
        ApplyFont(_titleLabel, _titleFont);
        lv_obj_set_style_text_color(_titleLabel, lv_color_hex(0x22183A), 0);

        _artistLabel = lv_label_create(_player);
        lv_obj_set_pos(_artistLabel, 30, 66);
        lv_obj_set_width(_artistLabel, 220);
        ApplyFont(_artistLabel, _bodyFont);
        lv_obj_set_style_text_color(_artistLabel, lv_color_hex(0x5D5675), 0);

        _genreLabel = lv_label_create(_player);
        lv_obj_set_pos(_genreLabel, 30, 88);
        lv_obj_set_width(_genreLabel, 220);
        ApplyFont(_genreLabel, _smallFont);
        lv_obj_set_style_text_color(_genreLabel, lv_color_hex(0xA9A2BC), 0);

        lv_obj_t* chip = CreateChipButton(_player, 30, 112, 28, 28, LV_SYMBOL_AUDIO, clickable: false, command: 0);
        lv_obj_set_style_bg_color(chip, lv_color_hex(0xF0E8FAu), 0);
        chip = CreateChipButton(_player, 70, 112, 28, 28, LV_SYMBOL_VOLUME_MAX, clickable: false, command: 0);
        lv_obj_set_style_bg_color(chip, lv_color_hex(0xF0E8FAu), 0);
        chip = CreateChipButton(_player, 110, 112, 28, 28, LV_SYMBOL_BELL, clickable: false, command: 0);
        lv_obj_set_style_bg_color(chip, lv_color_hex(0xF0E8FAu), 0);
        chip = CreateChipButton(_player, 150, 112, 28, 28, LV_SYMBOL_LIST, clickable: false, command: 0);
        lv_obj_set_style_bg_color(chip, lv_color_hex(0xF0E8FAu), 0);
    }

    private void CreateControlStrip()
    {
        _shuffleButton = CreateChipButton(_player, 30, 148, 30, 30, LV_SYMBOL_SHUFFLE, clickable: true, command: CommandToggleShuffle);
        _shuffleIcon = lv_obj_get_child(_shuffleButton, 0);

        _prevButton = CreateCircleButton(_player, 76, 145, 40, 40, LV_SYMBOL_PREV, CommandPrevTrack);
        _prevIcon = lv_obj_get_child(_prevButton, 0);

        _playButton = CreateCircleButton(_player, 126, 136, 58, 58, LV_SYMBOL_PLAY, CommandTogglePlay);
        _playIcon = lv_obj_get_child(_playButton, 0);

        _nextButton = CreateCircleButton(_player, 194, 145, 40, 40, LV_SYMBOL_NEXT, CommandNextTrack);
        _nextIcon = lv_obj_get_child(_nextButton, 0);

        _loopButton = CreateChipButton(_player, 250, 148, 30, 30, LV_SYMBOL_LOOP, clickable: true, command: CommandToggleLoop);
        _loopIcon = lv_obj_get_child(_loopButton, 0);

        _progressSlider = lv_slider_create(_player);
        lv_obj_set_pos(_progressSlider, 30, 200);
        lv_obj_set_size(_progressSlider, 210, 8);
        lv_slider_set_range(_progressSlider, 0, s_tracks[0].DurationSeconds);
        lv_obj_add_event_cb(_progressSlider, &OnCommandEvent, lv_event_code_t.LV_EVENT_VALUE_CHANGED, (void*)CommandSeekSlider);
        lv_obj_set_style_bg_color(_progressSlider, lv_color_hex(0xE8DEFA), LV_PART_MAIN);
        lv_obj_set_style_bg_opa(_progressSlider, 255, LV_PART_MAIN);
        lv_obj_set_style_radius(_progressSlider, 10, LV_PART_MAIN);
        lv_obj_set_style_bg_color(_progressSlider, lv_color_hex(0x6F8AF6), LV_PART_INDICATOR);
        lv_obj_set_style_bg_opa(_progressSlider, 255, LV_PART_INDICATOR);
        lv_obj_set_style_radius(_progressSlider, 10, LV_PART_INDICATOR);
        lv_obj_set_style_bg_color(_progressSlider, lv_color_hex(0xFFFFFF), LV_PART_KNOB);
        lv_obj_set_style_bg_opa(_progressSlider, 255, LV_PART_KNOB);
        lv_obj_set_style_radius(_progressSlider, LV_RADIUS_CIRCLE, LV_PART_KNOB);
        lv_obj_set_style_border_width(_progressSlider, 0, LV_PART_KNOB);
        lv_obj_set_style_shadow_width(_progressSlider, 14, LV_PART_KNOB);
        lv_obj_set_style_shadow_spread(_progressSlider, 0, LV_PART_KNOB);
        lv_obj_set_style_shadow_color(_progressSlider, lv_color_hex(0x7B6AF6), LV_PART_KNOB);
        lv_obj_set_style_shadow_opa(_progressSlider, 55, LV_PART_KNOB);

        _elapsedLabel = lv_label_create(_player);
        lv_obj_set_pos(_elapsedLabel, 30, 214);
        ApplyFont(_elapsedLabel, _smallFont);
        lv_obj_set_style_text_color(_elapsedLabel, lv_color_hex(0x7E7894), 0);

        _durationLabel = lv_label_create(_player);
        lv_obj_set_pos(_durationLabel, 208, 214);
        ApplyFont(_durationLabel, _smallFont);
        lv_obj_set_style_text_color(_durationLabel, lv_color_hex(0x7E7894), 0);
    }

    private void CreateAlbumArea()
    {
        _albumGlow = lv_obj_create(_player);
        lv_obj_set_pos(_albumGlow, 296, 28);
        lv_obj_set_size(_albumGlow, 156, 156);
        lv_obj_set_style_radius(_albumGlow, LV_RADIUS_CIRCLE, 0);
        lv_obj_set_style_bg_color(_albumGlow, lv_color_hex(0xFFFFFF), 0);
        lv_obj_set_style_bg_opa(_albumGlow, 255, 0);
        lv_obj_set_style_border_width(_albumGlow, 0, 0);
        lv_obj_set_style_shadow_width(_albumGlow, 42, 0);
        lv_obj_set_style_shadow_spread(_albumGlow, 4, 0);
        lv_obj_set_style_shadow_opa(_albumGlow, 70, 0);
        lv_obj_set_style_shadow_color(_albumGlow, lv_color_hex(0x6F8AF6), 0);
        lv_obj_clear_flag(_albumGlow, lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE);

        _albumRing = lv_obj_create(_player);
        lv_obj_set_pos(_albumRing, 304, 36);
        lv_obj_set_size(_albumRing, 140, 140);
        lv_obj_set_style_radius(_albumRing, LV_RADIUS_CIRCLE, 0);
        lv_obj_set_style_bg_color(_albumRing, lv_color_hex(0xF4ECFB), 0);
        lv_obj_set_style_bg_opa(_albumRing, 255, 0);
        lv_obj_set_style_border_width(_albumRing, 0, 0);
        lv_obj_set_style_outline_width(_albumRing, 2, 0);
        lv_obj_set_style_outline_opa(_albumRing, 110, 0);
        lv_obj_set_style_outline_pad(_albumRing, 5, 0);
        lv_obj_set_style_outline_color(_albumRing, lv_color_hex(0x6F8AF6), 0);
        lv_obj_clear_flag(_albumRing, lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE);

        _albumClip = lv_obj_create(_albumRing);
        lv_obj_set_size(_albumClip, CoverSize, CoverSize);
        lv_obj_center(_albumClip);
        lv_obj_set_style_radius(_albumClip, LV_RADIUS_CIRCLE, 0);
        lv_obj_set_style_clip_corner(_albumClip, true, 0);
        lv_obj_set_style_bg_color(_albumClip, lv_color_hex(0xFFFFFF), 0);
        lv_obj_set_style_bg_opa(_albumClip, 255, 0);
        lv_obj_set_style_border_width(_albumClip, 0, 0);
        lv_obj_set_style_pad_all(_albumClip, 0, 0);
        lv_obj_clear_flag(_albumClip, lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE);

        _albumImage = lv_image_create(_albumClip);
        lv_obj_set_size(_albumImage, CoverSize, CoverSize);
        lv_obj_center(_albumImage);
        lv_image_set_inner_align(_albumImage, lv_image_align_t.LV_IMAGE_ALIGN_CENTER);

        for (int index = 0; index < SpectrumBarCount; index++)
        {
            lv_obj_t* bar = lv_obj_create(_player);
            lv_obj_set_size(bar, 6, 18);
            lv_obj_set_style_radius(bar, 6, 0);
            lv_obj_set_style_border_width(bar, 0, 0);
            lv_obj_set_style_pad_all(bar, 0, 0);
            lv_obj_set_style_bg_color(bar, lv_color_hex(0x6F8AF6), 0);
            lv_obj_set_style_bg_opa(bar, 180, 0);
            lv_obj_clear_flag(bar, lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE);

            _spectrumBars[index] = new SpectrumBar
            {
                Object = bar,
                Angle = (360 / SpectrumBarCount) * index,
                Phase = 0.37f * index,
            };
        }
    }

    private void CreateFooter()
    {
        _footerButton = lv_button_create(_root);
        lv_obj_set_pos(_footerButton, 24, 242);
        lv_obj_set_size(_footerButton, 142, 22);
        lv_obj_add_event_cb(_footerButton, &OnCommandEvent, lv_event_code_t.LV_EVENT_CLICKED, (void*)CommandToggleList);
        lv_obj_set_style_bg_color(_footerButton, lv_color_hex(0x343247), 0);
        lv_obj_set_style_bg_opa(_footerButton, 0, 0);
        lv_obj_set_style_border_width(_footerButton, 0, 0);
        lv_obj_set_style_shadow_width(_footerButton, 0, 0);
        lv_obj_set_style_pad_all(_footerButton, 0, 0);

        lv_obj_t* footerIcon = lv_label_create(_footerButton);
        lv_obj_set_pos(footerIcon, 0, 2);
        ApplyFont(footerIcon, _iconFont);
        lv_obj_set_style_text_color(footerIcon, lv_color_hex(0xD8D1EF), 0);
        SetLabelText(footerIcon, LV_SYMBOL_LIST);

        _footerLabel = lv_label_create(_footerButton);
        lv_obj_set_pos(_footerLabel, 22, 2);
        ApplyFont(_footerLabel, _bodyFont);
        lv_obj_set_style_text_color(_footerLabel, lv_color_hex(0xFFFFFF), 0);
        SetLabelText(_footerLabel, "ALL TRACKS");

        _footerCountLabel = lv_label_create(_root);
        lv_obj_set_pos(_footerCountLabel, 346, 244);
        ApplyFont(_footerCountLabel, _smallFont);
        lv_obj_set_style_text_color(_footerCountLabel, lv_color_hex(0xA9A3BE), 0);
        SetLabelText(_footerCountLabel, $"{s_tracks.Length} tracks ready");
    }

    private void CreateTrackSheet()
    {
        _trackSheetMask = lv_obj_create(_root);
        lv_obj_set_size(_trackSheetMask, DemoWidth, DemoHeight);
        lv_obj_set_pos(_trackSheetMask, 0, 0);
        lv_obj_set_style_bg_color(_trackSheetMask, lv_color_hex(0x161321), 0);
        lv_obj_set_style_bg_opa(_trackSheetMask, 92, 0);
        lv_obj_set_style_border_width(_trackSheetMask, 0, 0);
        lv_obj_set_style_radius(_trackSheetMask, 0, 0);
        lv_obj_set_style_pad_all(_trackSheetMask, 0, 0);
        lv_obj_add_event_cb(_trackSheetMask, &OnCommandEvent, lv_event_code_t.LV_EVENT_CLICKED, (void*)CommandHideList);
        lv_obj_add_flag(_trackSheetMask, lv_obj_flag_t.LV_OBJ_FLAG_HIDDEN);

        _trackSheet = lv_obj_create(_root);
        lv_obj_set_pos(_trackSheet, 18, 16);
        lv_obj_set_size(_trackSheet, 442, 238);
        lv_obj_set_style_bg_color(_trackSheet, lv_color_hex(0x3A3750), 0);
        lv_obj_set_style_bg_grad_color(_trackSheet, lv_color_hex(0x2F2B43), 0);
        lv_obj_set_style_bg_grad_dir(_trackSheet, lv_grad_dir_t.LV_GRAD_DIR_VER, 0);
        lv_obj_set_style_bg_opa(_trackSheet, 248, 0);
        lv_obj_set_style_border_width(_trackSheet, 0, 0);
        lv_obj_set_style_radius(_trackSheet, 26, 0);
        lv_obj_set_style_pad_all(_trackSheet, 0, 0);
        lv_obj_set_style_shadow_width(_trackSheet, 24, 0);
        lv_obj_set_style_shadow_spread(_trackSheet, 1, 0);
        lv_obj_set_style_shadow_color(_trackSheet, lv_color_hex(0x171422), 0);
        lv_obj_set_style_shadow_opa(_trackSheet, 80, 0);
        lv_obj_add_flag(_trackSheet, lv_obj_flag_t.LV_OBJ_FLAG_HIDDEN);

        lv_obj_t* sheetTitle = lv_label_create(_trackSheet);
        lv_obj_set_pos(sheetTitle, 22, 18);
        ApplyFont(sheetTitle, _bodyFont);
        lv_obj_set_style_text_color(sheetTitle, lv_color_hex(0xFFFFFF), 0);
        SetLabelText(sheetTitle, "ALL TRACKS");

        lv_obj_t* sheetSubtitle = lv_label_create(_trackSheet);
        lv_obj_set_pos(sheetSubtitle, 22, 40);
        ApplyFont(sheetSubtitle, _smallFont);
        lv_obj_set_style_text_color(sheetSubtitle, lv_color_hex(0xB8B2CC), 0);
        SetLabelText(sheetSubtitle, "Pure C# playback host with live controls");

        lv_obj_t* closeButton = CreateChipButton(_trackSheet, 392, 16, 30, 30, LV_SYMBOL_CLOSE, clickable: true, command: CommandCloseList);
        lv_obj_set_style_bg_color(closeButton, lv_color_hex(0x4B4761), 0);

        _trackList = lv_obj_create(_trackSheet);
        lv_obj_set_pos(_trackList, 18, 70);
        lv_obj_set_size(_trackList, 406, 150);
        lv_obj_set_style_bg_opa(_trackList, 0, 0);
        lv_obj_set_style_border_width(_trackList, 0, 0);
        lv_obj_set_style_pad_top(_trackList, 0, 0);
        lv_obj_set_style_pad_bottom(_trackList, 0, 0);
        lv_obj_set_style_pad_left(_trackList, 0, 0);
        lv_obj_set_style_pad_right(_trackList, 4, 0);
        lv_obj_set_style_pad_row(_trackList, 8, 0);
        lv_obj_set_scroll_dir(_trackList, lv_dir_t.LV_DIR_VER);
        lv_obj_set_scrollbar_mode(_trackList, lv_scrollbar_mode_t.LV_SCROLLBAR_MODE_OFF);
        lv_obj_set_flex_flow(_trackList, lv_flex_flow_t.LV_FLEX_FLOW_COLUMN);

        for (int index = 0; index < s_tracks.Length; index++)
        {
            lv_obj_t* rowButton = lv_button_create(_trackList);
            lv_obj_set_size(rowButton, lv_pct(100), 50);
            lv_obj_add_event_cb(rowButton, &OnCommandEvent, lv_event_code_t.LV_EVENT_CLICKED, (void*)(CommandTrackBase + index));
            lv_obj_set_style_radius(rowButton, 18, 0);
            lv_obj_set_style_border_width(rowButton, 0, 0);
            lv_obj_set_style_shadow_width(rowButton, 0, 0);
            lv_obj_set_style_pad_all(rowButton, 0, 0);

            lv_obj_t* icon = lv_label_create(rowButton);
            lv_obj_set_pos(icon, 12, 14);
            ApplyFont(icon, _iconFont);

            lv_obj_t* title = lv_label_create(rowButton);
            lv_obj_set_pos(title, 42, 8);
            lv_obj_set_width(title, 250);
            ApplyFont(title, _bodyFont);
            lv_label_set_long_mode(title, LV_LABEL_LONG_CLIP);
            SetLabelText(title, s_tracks[index].Title);

            lv_obj_t* meta = lv_label_create(rowButton);
            lv_obj_set_pos(meta, 42, 26);
            lv_obj_set_width(meta, 250);
            ApplyFont(meta, _smallFont);
            lv_label_set_long_mode(meta, LV_LABEL_LONG_CLIP);
            SetLabelText(meta, s_tracks[index].Artist);

            lv_obj_t* duration = lv_label_create(rowButton);
            lv_obj_set_pos(duration, 332, 16);
            ApplyFont(duration, _smallFont);
            SetLabelText(duration, FormatTime(s_tracks[index].DurationSeconds));

            _trackRows[index] = new TrackRow
            {
                Button = rowButton,
                Icon = icon,
                Title = title,
                Meta = meta,
                Duration = duration,
            };
        }
    }

    private void StartUiTimer()
    {
        _lastTickMs = Environment.TickCount64;
        _uiTimer = lv_timer_create(&OnUiTimer, 33, null);
    }

    private void Tick()
    {
        long now = Environment.TickCount64;
        int elapsed = (int)Math.Clamp(now - _lastTickMs, 0, 100);
        _lastTickMs = now;

        if (_playing)
        {
            _playbackMilliseconds += elapsed;
            if (_playbackMilliseconds >= CurrentTrack.DurationSeconds * 1000)
            {
                OnTrackCompleted();
            }
        }

        UpdateProgressDisplay();
        UpdateSpectrum(now);
    }

    private void OnTrackCompleted()
    {
        if (_loopEnabled)
        {
            _playbackMilliseconds = 0;
            return;
        }

        LoadTrack(GetAdjacentTrackIndex(1), startPlayback: true, resetProgress: true, animateCover: true);
    }

    private void LoadTrack(int index, bool startPlayback, bool resetProgress, bool animateCover)
    {
        _currentTrackIndex = NormalizeTrackIndex(index);
        if (resetProgress)
        {
            _playbackMilliseconds = 0;
        }

        _playing = startPlayback;
        TrackInfo track = CurrentTrack;

        SetLabelText(_titleLabel, track.Title);
        SetLabelText(_artistLabel, track.Artist);
        SetLabelText(_genreLabel, track.Genre);
        SetLabelText(_durationLabel, FormatTime(track.DurationSeconds));

        _ignoreSliderEvents = true;
        lv_slider_set_range(_progressSlider, 0, track.DurationSeconds);
        lv_slider_set_value(_progressSlider, 0, LV_ANIM_OFF);
        _ignoreSliderEvents = false;

        if (_coverDescriptors.Length > 0)
        {
            lv_image_set_src(_albumImage, _coverDescriptors[_currentTrackIndex].Descriptor);
        }

        ApplyTheme(track);
        UpdatePlaybackVisualState();
        UpdateTrackRows();
        UpdateProgressDisplay();

        if (animateCover)
        {
            AnimateCoverPop();
        }
    }

    private void ApplyTheme(TrackInfo track)
    {
        uint accent = track.AccentColor;
        uint primary = track.PrimaryColor;
        uint secondary = track.SecondaryColor;

        lv_obj_set_style_bg_color(_player, lv_color_hex(0xFFFEFF), 0);
        lv_obj_set_style_bg_grad_color(_player, lv_color_hex(BlendColor(accent, 0xFFFFFF, 0.88f)), 0);
        lv_obj_set_style_bg_grad_dir(_player, lv_grad_dir_t.LV_GRAD_DIR_VER, 0);

        lv_obj_set_style_bg_color(_topBlob, lv_color_hex(BlendColor(primary, 0xFFFFFF, 0.85f)), 0);
        lv_obj_set_style_bg_color(_bottomBlobLeft, lv_color_hex(BlendColor(accent, 0xFFFFFF, 0.90f)), 0);
        lv_obj_set_style_bg_color(_bottomBlobRight, lv_color_hex(BlendColor(secondary, 0xFFFFFF, 0.89f)), 0);

        lv_obj_set_style_text_color(_genreLabel, lv_color_hex(BlendColor(accent, 0x7A748F, 0.35f)), 0);
        lv_obj_set_style_bg_color(_playButton, lv_color_hex(accent), 0);
        lv_obj_set_style_shadow_color(_playButton, lv_color_hex(accent), 0);
        lv_obj_set_style_shadow_color(_progressSlider, lv_color_hex(accent), LV_PART_KNOB);
        lv_obj_set_style_bg_color(_progressSlider, lv_color_hex(accent), LV_PART_INDICATOR);
        lv_obj_set_style_shadow_color(_albumGlow, lv_color_hex(accent), 0);
        lv_obj_set_style_outline_color(_albumRing, lv_color_hex(BlendColor(accent, 0xFFFFFF, 0.28f)), 0);

        lv_obj_set_style_text_color(_footerLabel, lv_color_hex(0xFFFFFF), 0);
        lv_obj_set_style_text_color(_footerCountLabel, lv_color_hex(BlendColor(accent, 0xCFC8E5, 0.72f)), 0);
    }

    private void UpdatePlaybackVisualState()
    {
        SetLabelText(_playIcon, _playing ? LV_SYMBOL_PAUSE : LV_SYMBOL_PLAY);
        lv_obj_set_style_text_color(_playIcon, lv_color_hex(0xFFFFFF), 0);

        UpdateToggleButton(_shuffleButton, _shuffleIcon, _shuffleEnabled);
        UpdateToggleButton(_loopButton, _loopIcon, _loopEnabled);
    }

    private void UpdateToggleButton(lv_obj_t* button, lv_obj_t* icon, bool active)
    {
        uint bgColor = active ? CurrentTrack.AccentColor : 0xF0E8FA;
        uint textColor = active ? 0xFFFFFFu : 0x786F92u;

        lv_obj_set_style_bg_opa(button, 255, 0);
        lv_obj_set_style_bg_color(button, lv_color_hex(bgColor), 0);
        lv_obj_set_style_text_color(icon, lv_color_hex(textColor), 0);
    }

    private void UpdateProgressDisplay()
    {
        int totalSeconds = CurrentTrack.DurationSeconds;
        int currentSeconds = Math.Clamp(_playbackMilliseconds / 1000, 0, totalSeconds);

        SetLabelText(_elapsedLabel, FormatTime(currentSeconds));

        _ignoreSliderEvents = true;
        lv_slider_set_value(_progressSlider, currentSeconds, LV_ANIM_OFF);
        _ignoreSliderEvents = false;
    }

    private void UpdateSpectrum(long now)
    {
        TrackInfo track = CurrentTrack;
        float time = (now % 100000) / 1000f;
        float playbackT = _playbackMilliseconds / 1000f;

        const int centerX = 374;
        const int centerY = 106;
        const int ringRadius = 74;

        for (int index = 0; index < _spectrumBars.Length; index++)
        {
            ref SpectrumBar bar = ref _spectrumBars[index];
            if (bar.Object == null)
            {
                continue;
            }

            float energy = ComputeSpectrumEnergy(index, time, playbackT, track.Seed, _playing);
            int height = 12 + (int)(energy * 26f);
            int width = 6;
            int y = centerY - ringRadius - height;
            int x = centerX - (width / 2);
            uint color = BlendColor(track.AccentColor, index % 2 == 0 ? track.PrimaryColor : 0xFFFFFF, 0.45f + (energy * 0.35f));

            lv_obj_set_size(bar.Object, width, height);
            lv_obj_set_pos(bar.Object, x, y);
            lv_obj_set_style_bg_color(bar.Object, lv_color_hex(color), 0);
            lv_obj_set_style_bg_opa(bar.Object, (byte)(100 + (energy * 155f)), 0);
            lv_obj_set_style_transform_pivot_x(bar.Object, width / 2, 0);
            lv_obj_set_style_transform_pivot_y(bar.Object, height + ringRadius, 0);
            lv_obj_set_style_transform_rotation(bar.Object, bar.Angle * 10, 0);
        }

        uint imageScale = 256u + (uint)(ComputeSpectrumEnergy(0, time, playbackT, track.Seed, _playing) * (_playing ? 20f : 8f));
        lv_image_set_scale(_albumImage, imageScale);
    }

    private void UpdateTrackRows()
    {
        for (int index = 0; index < _trackRows.Length; index++)
        {
            TrackRow row = _trackRows[index];
            bool selected = index == _currentTrackIndex;

            lv_obj_set_style_bg_opa(row.Button, selected ? (byte)255 : (byte)0, 0);
            lv_obj_set_style_bg_color(row.Button, lv_color_hex(selected ? 0x4D4967u : 0x000000u), 0);

            if (selected)
            {
                lv_obj_set_style_text_color(row.Icon, lv_color_hex(BlendColor(CurrentTrack.AccentColor, 0xFFFFFF, 0.45f)), 0);
                lv_obj_set_style_text_color(row.Title, lv_color_hex(0xFFFFFF), 0);
                lv_obj_set_style_text_color(row.Meta, lv_color_hex(0xC8C2DB), 0);
                lv_obj_set_style_text_color(row.Duration, lv_color_hex(0xFFFFFF), 0);
                SetLabelText(row.Icon, _playing ? LV_SYMBOL_PAUSE : LV_SYMBOL_PLAY);
            }
            else
            {
                lv_obj_set_style_text_color(row.Icon, lv_color_hex(0xA7A1BC), 0);
                lv_obj_set_style_text_color(row.Title, lv_color_hex(0xF4F1FA), 0);
                lv_obj_set_style_text_color(row.Meta, lv_color_hex(0xA8A2BE), 0);
                lv_obj_set_style_text_color(row.Duration, lv_color_hex(0xC6C0D7), 0);
                SetLabelText(row.Icon, LV_SYMBOL_PLAY);
            }
        }

        lv_obj_scroll_to_view(_trackRows[_currentTrackIndex].Button, LV_ANIM_ON);
    }

    private void SetTrackSheetVisible(bool visible)
    {
        if (_trackSheetVisible == visible)
        {
            return;
        }

        _trackSheetVisible = visible;

        if (visible)
        {
            lv_obj_clear_flag(_trackSheetMask, lv_obj_flag_t.LV_OBJ_FLAG_HIDDEN);
            lv_obj_clear_flag(_trackSheet, lv_obj_flag_t.LV_OBJ_FLAG_HIDDEN);
            lv_obj_fade_in(_trackSheetMask, 140, 0);
            lv_obj_fade_in(_trackSheet, 180, 0);
        }
        else
        {
            lv_obj_add_flag(_trackSheetMask, lv_obj_flag_t.LV_OBJ_FLAG_HIDDEN);
            lv_obj_add_flag(_trackSheet, lv_obj_flag_t.LV_OBJ_FLAG_HIDDEN);
        }
    }

    private void TogglePlay()
    {
        _playing = !_playing;
        UpdatePlaybackVisualState();
        UpdateTrackRows();
    }

    private void ToggleShuffle()
    {
        _shuffleEnabled = !_shuffleEnabled;
        UpdatePlaybackVisualState();
    }

    private void ToggleLoop()
    {
        _loopEnabled = !_loopEnabled;
        UpdatePlaybackVisualState();
    }

    private void SelectPreviousTrack()
    {
        LoadTrack(GetAdjacentTrackIndex(-1), startPlayback: _playing, resetProgress: true, animateCover: true);
    }

    private void SelectNextTrack()
    {
        LoadTrack(GetAdjacentTrackIndex(1), startPlayback: _playing, resetProgress: true, animateCover: true);
    }

    private void SelectTrack(int trackIndex)
    {
        LoadTrack(trackIndex, startPlayback: true, resetProgress: true, animateCover: true);
        SetTrackSheetVisible(false);
    }

    private void SeekTo(int second)
    {
        _playbackMilliseconds = Math.Clamp(second, 0, CurrentTrack.DurationSeconds) * 1000;
        UpdateProgressDisplay();
    }

    private int GetAdjacentTrackIndex(int direction)
    {
        if (_shuffleEnabled)
        {
            int candidate;
            do
            {
                candidate = Random.Shared.Next(s_tracks.Length);
            }
            while (s_tracks.Length > 1 && candidate == _currentTrackIndex);

            return candidate;
        }

        return NormalizeTrackIndex(_currentTrackIndex + direction);
    }

    private int NormalizeTrackIndex(int index)
    {
        int count = s_tracks.Length;
        int normalized = index % count;
        return normalized < 0 ? normalized + count : normalized;
    }

    private void AnimateCoverPop()
    {
        lv_anim_t animation;
        lv_anim_init(&animation);
        lv_anim_set_var(&animation, _albumImage);
        lv_anim_set_values(&animation, 218, 256);
        lv_anim_set_duration(&animation, 220);
        lv_anim_set_exec_cb(&animation, &ImageScaleAnimation);
        lv_anim_start(&animation);
    }

    private void CleanupResources()
    {
        if (_uiTimer != null)
        {
            lv_timer_delete(_uiTimer);
            _uiTimer = null;
        }

        DisposeCoverDescriptors();

        foreach (var manager in _fontManagers)
        {
            manager.Dispose();
        }

        _fontManagers.Clear();
        _titleFont = null;
        _bodyFont = null;
        _smallFont = null;
        _iconFont = null;
        s_activeDemo = null;
    }

    private void DisposeCoverDescriptors()
    {
        foreach (var descriptor in _coverDescriptors)
        {
            descriptor.Dispose();
        }

        _coverDescriptors = Array.Empty<ImageDescriptor>();
    }

    private SixLaborsFontManager RegisterFontManager(SixLaborsFontManager manager)
    {
        _fontManagers.Add(manager);
        return manager;
    }

    private void ApplyFont(lv_obj_t* obj, SixLaborsFontManager? manager)
    {
        if (obj != null && manager is not null)
        {
            lv_obj_set_style_text_font(obj, manager.GetLvFontPtr(), 0);
        }
    }

    private static FontFamily? ResolveFontFamily()
    {
        string[] candidateNames =
        [
            "Segoe UI",
            "Microsoft YaHei",
            "Microsoft YaHei UI",
            "Noto Sans CJK SC",
            "Noto Sans",
            "DejaVu Sans",
            "Liberation Sans",
            "Arial",
        ];

        foreach (string candidateName in candidateNames)
        {
            if (SystemFonts.TryGet(candidateName, out FontFamily family))
            {
                return family;
            }
        }

        foreach (FontFamily family in SystemFonts.Families)
        {
            return family;
        }

        return null;
    }

    private static void PaintCoverImage(ImageSharpImage image, TrackInfo track)
    {
        image.ProcessPixelRows(accessor =>
        {
            int width = accessor.Width;
            int height = accessor.Height;
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                float fy = (y - halfHeight) / halfHeight;

                for (int x = 0; x < width; x++)
                {
                    float fx = (x - halfWidth) / halfWidth;
                    float radial = MathF.Sqrt((fx * fx) + (fy * fy));
                    float diagonal = (fx - fy + 2f) / 4f;
                    float swirl = 0.5f + (0.5f * MathF.Sin(((fx * 7.4f) + (fy * 5.2f) + track.Seed) * 1.35f));
                    float bloom = MathF.Max(0f, 1.2f - (radial * 1.35f));
                    float stripe = MathF.Max(0f, 1f - MathF.Abs((fx * 0.75f) + (fy * 1.2f) - 0.15f) * 2.3f);

                    uint baseColor = BlendColor(track.PrimaryColor, track.SecondaryColor, diagonal);
                    uint litColor = BlendColor(baseColor, track.AccentColor, (bloom * 0.55f) + (stripe * 0.22f));
                    uint finalColor = BlendColor(litColor, 0xFFFFFF, 0.06f + (swirl * 0.12f));

                    if (radial > 1f)
                    {
                        row[x] = new Rgba32(0, 0, 0, 0);
                        continue;
                    }

                    row[x] = ToPixel(finalColor);
                }
            }
        });
    }

    private static ImageDescriptor CreateImageDescriptor(ImageSharpImage image)
    {
        byte[] rgbaBytes = GC.AllocateUninitializedArray<byte>(image.Width * image.Height * Unsafe.SizeOf<Rgba32>());
        byte[] bgraBytes = GC.AllocateUninitializedArray<byte>(rgbaBytes.Length);

        image.CopyPixelDataTo(MemoryMarshal.Cast<byte, Rgba32>(rgbaBytes.AsSpan()));
        ConvertRgbaToBgra(rgbaBytes, bgraBytes);

        nuint descriptorSize = (nuint)sizeof(lv_image_dsc_t);
        nuint totalSize = descriptorSize + (nuint)bgraBytes.Length;
        byte* buffer = (byte*)NativeMemory.Alloc(totalSize);
        if (buffer == null)
        {
            throw new OutOfMemoryException("Unable to allocate image descriptor for MusicDemo.");
        }

        var descriptor = (lv_image_dsc_t*)buffer;
        byte* data = buffer + descriptorSize;

        fixed (byte* source = bgraBytes)
        {
            Buffer.MemoryCopy(source, data, bgraBytes.Length, bgraBytes.Length);
        }

        *descriptor = new lv_image_dsc_t
        {
            header = new lv_image_header_t
            {
                magic = LvImageHeaderMagic,
                cf = (uint)lv_color_format_t.LV_COLOR_FORMAT_ARGB8888,
                flags = 0,
                w = (uint)image.Width,
                h = (uint)image.Height,
                stride = (uint)(image.Width * 4),
            },
            data_size = (uint)bgraBytes.Length,
            data = data,
        };

        return new ImageDescriptor((nint)buffer, descriptor);
    }

    private static void ConvertRgbaToBgra(ReadOnlySpan<byte> rgbaBytes, Span<byte> bgraBytes)
    {
        for (int offset = 0; offset < rgbaBytes.Length; offset += 4)
        {
            bgraBytes[offset] = rgbaBytes[offset + 2];
            bgraBytes[offset + 1] = rgbaBytes[offset + 1];
            bgraBytes[offset + 2] = rgbaBytes[offset];
            bgraBytes[offset + 3] = rgbaBytes[offset + 3];
        }
    }

    private static float ComputeSpectrumEnergy(int index, float time, float playbackTime, int seed, bool playing)
    {
        float pace = playing ? 1f : 0.26f;
        float waveA = 0.5f + (0.5f * MathF.Sin(((time * pace * 3.2f) + (index * 0.55f) + (seed * 0.1f))));
        float waveB = 0.5f + (0.5f * MathF.Sin(((playbackTime * 1.6f) + (index * 0.9f) + seed) * 0.73f));
        float waveC = 0.5f + (0.5f * MathF.Cos(((time * pace * 1.9f) + (index * 0.33f) + (seed * 0.07f))));
        float blend = (waveA * 0.55f) + (waveB * 0.30f) + (waveC * 0.15f);

        return Math.Clamp(blend * (playing ? 1f : 0.55f), 0.08f, 1f);
    }

    private static lv_obj_t* CreateBlob(lv_obj_t* parent, int x, int y, int width, int height, uint color, byte opacity)
    {
        lv_obj_t* blob = lv_obj_create(parent);
        lv_obj_set_pos(blob, x, y);
        lv_obj_set_size(blob, width, height);
        lv_obj_set_style_bg_color(blob, lv_color_hex(color), 0);
        lv_obj_set_style_bg_opa(blob, opacity, 0);
        lv_obj_set_style_border_width(blob, 0, 0);
        lv_obj_set_style_radius(blob, 80, 0);
        lv_obj_set_style_pad_all(blob, 0, 0);
        lv_obj_clear_flag(blob, lv_obj_flag_t.LV_OBJ_FLAG_SCROLLABLE);
        return blob;
    }

    private lv_obj_t* CreateChipButton(lv_obj_t* parent, int x, int y, int width, int height, ReadOnlySpan<byte> symbol, bool clickable, int command)
    {
        lv_obj_t* button = lv_button_create(parent);
        lv_obj_set_pos(button, x, y);
        lv_obj_set_size(button, width, height);
        if (clickable)
        {
            lv_obj_add_event_cb(button, &OnCommandEvent, lv_event_code_t.LV_EVENT_CLICKED, (void*)command);
        }

        lv_obj_set_style_radius(button, 14, 0);
        lv_obj_set_style_border_width(button, 0, 0);
        lv_obj_set_style_shadow_width(button, 0, 0);
        lv_obj_set_style_pad_all(button, 0, 0);
        lv_obj_set_style_bg_color(button, lv_color_hex(0xF0E8FA), 0);
        lv_obj_set_style_bg_opa(button, 255, 0);

        lv_obj_t* icon = lv_label_create(button);
        ApplyFont(icon, _iconFont);
        lv_obj_set_style_text_color(icon, lv_color_hex(0x786F92), 0);
        SetLabelText(icon, symbol);
        lv_obj_center(icon);
        return button;
    }

    private lv_obj_t* CreateCircleButton(lv_obj_t* parent, int x, int y, int width, int height, ReadOnlySpan<byte> symbol, int command)
    {
        lv_obj_t* button = lv_button_create(parent);
        lv_obj_set_pos(button, x, y);
        lv_obj_set_size(button, width, height);
        lv_obj_add_event_cb(button, &OnCommandEvent, lv_event_code_t.LV_EVENT_CLICKED, (void*)command);
        lv_obj_set_style_radius(button, LV_RADIUS_CIRCLE, 0);
        lv_obj_set_style_border_width(button, 0, 0);
        lv_obj_set_style_pad_all(button, 0, 0);
        lv_obj_set_style_shadow_width(button, width > 40 ? 22 : 12, 0);
        lv_obj_set_style_shadow_spread(button, 0, 0);
        lv_obj_set_style_shadow_opa(button, (byte)(width > 40 ? 55 : 35), 0);
        lv_obj_set_style_bg_color(button, lv_color_hex(width > 40 ? 0x6F8AF6u : 0xEEE7FAu), 0);

        lv_obj_t* icon = lv_label_create(button);
        ApplyFont(icon, width > 40 ? _iconFont : _bodyFont);
        lv_obj_set_style_text_color(icon, lv_color_hex(width > 40 ? 0xFFFFFFu : 0x564F6Eu), 0);
        SetLabelText(icon, symbol);
        lv_obj_center(icon);
        return button;
    }

    private static void SetLabelText(lv_obj_t* label, string text)
    {
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(text + '\0');
        fixed (byte* ptr = utf8Bytes)
        {
            lv_label_set_text(label, ptr);
        }
    }

    private static void SetLabelText(lv_obj_t* label, ReadOnlySpan<byte> utf8Text)
    {
        fixed (byte* ptr = utf8Text)
        {
            lv_label_set_text(label, ptr);
        }
    }

    private static string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:00}";
    }

    private TrackInfo CurrentTrack => s_tracks[_currentTrackIndex];

    private static uint BlendColor(uint left, uint right, float amount)
    {
        float t = Math.Clamp(amount, 0f, 1f);
        byte lr = (byte)((left >> 16) & 0xFF);
        byte lg = (byte)((left >> 8) & 0xFF);
        byte lb = (byte)(left & 0xFF);

        byte rr = (byte)((right >> 16) & 0xFF);
        byte rg = (byte)((right >> 8) & 0xFF);
        byte rb = (byte)(right & 0xFF);

        byte br = (byte)(lr + ((rr - lr) * t));
        byte bg = (byte)(lg + ((rg - lg) * t));
        byte bb = (byte)(lb + ((rb - lb) * t));

        return (uint)((br << 16) | (bg << 8) | bb);
    }

    private static Rgba32 ToPixel(uint color)
    {
        return new Rgba32(
            (byte)((color >> 16) & 0xFF),
            (byte)((color >> 8) & 0xFF),
            (byte)(color & 0xFF),
            255);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnUiTimer(lv_timer_t* timer)
    {
        _ = timer;
        s_activeDemo?.Tick();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void OnCommandEvent(lv_event_t* e)
    {
        frmMusicDemo? demo = s_activeDemo;
        if (demo is null)
        {
            return;
        }

        lv_event_code_t code = lv_event_get_code(e);
        int command = (int)(nint)lv_event_get_user_data(e);

        if (command == CommandSeekSlider)
        {
            if (code == lv_event_code_t.LV_EVENT_VALUE_CHANGED && !demo._ignoreSliderEvents)
            {
                lv_obj_t* slider = lv_event_get_target_obj(e);
                demo.SeekTo(lv_slider_get_value(slider));
            }

            return;
        }

        if (code != lv_event_code_t.LV_EVENT_CLICKED)
        {
            return;
        }

        switch (command)
        {
            case CommandToggleList:
                demo.SetTrackSheetVisible(!demo._trackSheetVisible);
                return;
            case CommandHideList:
            case CommandCloseList:
                demo.SetTrackSheetVisible(false);
                return;
            case CommandTogglePlay:
                demo.TogglePlay();
                return;
            case CommandPrevTrack:
                demo.SelectPreviousTrack();
                return;
            case CommandNextTrack:
                demo.SelectNextTrack();
                return;
            case CommandToggleShuffle:
                demo.ToggleShuffle();
                return;
            case CommandToggleLoop:
                demo.ToggleLoop();
                return;
        }

        if (command >= CommandTrackBase)
        {
            demo.SelectTrack(command - CommandTrackBase);
        }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static void ImageScaleAnimation(void* obj, int value)
    {
        lv_image_set_scale((lv_obj_t*)obj, (uint)value);
    }

    private readonly struct ImageDescriptor : IDisposable
    {
        public ImageDescriptor(nint buffer, lv_image_dsc_t* descriptor)
        {
            Buffer = buffer;
            Descriptor = descriptor;
        }

        public nint Buffer { get; }

        public lv_image_dsc_t* Descriptor { get; }

        public void Dispose()
        {
            if (Buffer != nint.Zero)
            {
                NativeMemory.Free((void*)Buffer);
            }
        }
    }

    private struct SpectrumBar
    {
        public lv_obj_t* Object;
        public int Angle;
        public float Phase;
    }

    private struct TrackRow
    {
        public lv_obj_t* Button;
        public lv_obj_t* Icon;
        public lv_obj_t* Title;
        public lv_obj_t* Meta;
        public lv_obj_t* Duration;
    }

    private sealed record TrackInfo(
        string Title,
        string Artist,
        string Genre,
        int DurationSeconds,
        uint PrimaryColor,
        uint SecondaryColor,
        uint AccentColor,
        int Seed);
}
