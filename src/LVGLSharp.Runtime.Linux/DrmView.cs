using LVGLSharp;
using LVGLSharp.Interop;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace LVGLSharp.Runtime.Linux;

public unsafe sealed class DrmView : ViewLifetimeBase
{
    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_create")]
    private static extern lv_display_t* lv_linux_drm_create_native();

    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_set_file")]
    private static extern lv_result_t lv_linux_drm_set_file_native(lv_display_t* display, byte* file, long connectorId);

    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_find_device_path")]
    private static extern byte* lv_linux_drm_find_device_path_native();

    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_set_mode_cb")]
    private static extern void lv_linux_drm_set_mode_cb_native(lv_display_t* display, delegate* unmanaged[Cdecl]<lv_display_t*, IntPtr, nuint, nuint> callback);

    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_mode_get_horizontal_resolution")]
    private static extern int lv_linux_drm_mode_get_horizontal_resolution_native(IntPtr mode);

    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_mode_get_vertical_resolution")]
    private static extern int lv_linux_drm_mode_get_vertical_resolution_native(IntPtr mode);

    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_mode_get_refresh_rate")]
    private static extern int lv_linux_drm_mode_get_refresh_rate_native(IntPtr mode);

    [DllImport("LVGL", CallingConvention = CallingConvention.Cdecl, EntryPoint = "lv_linux_drm_mode_is_preferred")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static extern bool lv_linux_drm_mode_is_preferred_native(IntPtr mode);

    private readonly string _devicePath;
    private readonly int _connectorId;
    private readonly float _dpi;
    private readonly DrmModePreference _modePreference;
    private bool _running;
    private bool _initialized;
    private lv_display_t* _display;
    private lv_obj_t* _root;
    private lv_group_t* _keyInputGroup;
    private lv_font_t* _fallbackFont;
    private lv_style_t* _defaultFontStyle;
    private SixLaborsFontManager? _fontManager;
    private string? _resolvedDevicePath;
    private string? _fontDiagnosticSummary;
    private string? _fontGlyphDiagnosticSummary;
    private static DrmView? s_activeModeSelectorOwner;

    public DrmView(string devicePath = "/dev/dri/card0", int connectorId = -1, float dpi = 96f, DrmModePreference? modePreference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(devicePath);

        _devicePath = devicePath;
        _connectorId = connectorId;
        _dpi = dpi;
        _modePreference = modePreference ?? DrmModePreference.Default;
    }

    public override lv_obj_t* Root => _root;

    public override lv_group_t* KeyInputGroup => _keyInputGroup;

    public override delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCallback => null;

    protected override void OnOpenCore()
    {
        s_activeModeSelectorOwner = this;
        LvglNativeLibraryResolver.EnsureRegistered();

        if (!lv_is_initialized())
        {
            lv_init();
        }

        _display = lv_linux_drm_create_native();
        if (_display == null)
        {
            throw new InvalidOperationException("DRM display 创建失败。");
        }

        _resolvedDevicePath = ResolveDevicePath();

        if (_modePreference.HasCustomSelector)
        {
            lv_linux_drm_set_mode_cb_native(_display, &SelectMode);
        }

        fixed (byte* devicePtr = Encoding.UTF8.GetBytes($"{_resolvedDevicePath}\0"))
        {
            var result = lv_linux_drm_set_file_native(_display, devicePtr, _connectorId);
            if (result != LV_RESULT_OK)
            {
                throw new InvalidOperationException($"DRM 设备初始化失败。Device={_resolvedDevicePath}, Connector={_connectorId}, Result={result}.");
            }
        }

        _root = lv_scr_act();
        _keyInputGroup = lv_group_create();
        _fallbackFont = lv_obj_get_style_text_font(_root, lv_part_t.LV_PART_MAIN);
        _fontDiagnosticSummary = LinuxSystemFontResolver.GetFontPathDiagnosticSummary();
        _fontGlyphDiagnosticSummary = LinuxSystemFontResolver.GetGlyphDiagnosticSummary();

        var systemFontPath = LinuxSystemFontResolver.TryResolveFontPath();
        if (!string.IsNullOrWhiteSpace(systemFontPath))
        {
            _fontManager = new SixLaborsFontManager(
                systemFontPath,
                12,
                _dpi,
                _fallbackFont,
                LvglHostDefaults.CreateDefaultFontFallbackGlyphs());

            _defaultFontStyle = LvglHostDefaults.ApplyDefaultFontStyle(_root, _fontManager.GetLvFontPtr());
        }

        _running = true;
        _initialized = true;
    }

    public override void HandleEvents()
    {
        if (!_initialized)
        {
            return;
        }

        lv_timer_handler();
    }

    protected override void RunLoopCore(Action iteration)
    {
        while (_running)
        {
            HandleEvents();
            iteration?.Invoke();
            Thread.Sleep(5);
        }
    }

    protected override void OnCloseCore()
    {
        if (s_activeModeSelectorOwner == this)
        {
            s_activeModeSelectorOwner = null;
        }

        _running = false;

        if (_keyInputGroup != null)
        {
            lv_group_delete(_keyInputGroup);
            _keyInputGroup = null;
        }

        if (_display != null)
        {
            lv_display_delete(_display);
            _display = null;
        }

        _fontManager?.Dispose();
        _fontManager = null;
        _resolvedDevicePath = null;
        _fontDiagnosticSummary = null;
        _fontGlyphDiagnosticSummary = null;
        _root = null;
        _initialized = false;
    }

    public override void RegisterTextInput(lv_obj_t* textArea)
    {
    }

    protected override bool CanSkipClose() => !_initialized && !_running && _display == null;

    public override string ToString()
    {
        var modeText = _modePreference.HasCustomSelector
            ? $"ModeSelector={_modePreference}"
            : "ModeSelector=default-preferred";

        return $"Host=DRM, Device={_resolvedDevicePath ?? _devicePath}, Connector={_connectorId}, Dpi={_dpi:0.##}, Running={_running}, Initialized={_initialized}, Display={_display != null}, Root={_root != null}, KeyGroup={_keyInputGroup != null}, {modeText}, FontDiag={_fontDiagnosticSummary ?? "<unresolved>"}, GlyphDiag={_fontGlyphDiagnosticSummary ?? "<unresolved>"}";
    }

    private string ResolveDevicePath()
    {
        if (!string.IsNullOrWhiteSpace(_devicePath) && _devicePath != "/dev/dri/card0")
        {
            return _devicePath;
        }

        var nativePath = lv_linux_drm_find_device_path_native();
        if (nativePath != null)
        {
            return Marshal.PtrToStringUTF8((IntPtr)nativePath) ?? _devicePath;
        }

        return _devicePath;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static nuint SelectMode(lv_display_t* display, IntPtr modes, nuint modeCount)
    {
        if (display == null || modes == IntPtr.Zero || modeCount == 0)
        {
            return 0;
        }

        var view = s_activeModeSelectorOwner;
        if (view is null || !view._modePreference.HasCustomSelector)
        {
            return 0;
        }

        nuint preferredIndex = 0;
        long preferredScore = long.MinValue;

        for (nuint i = 0; i < modeCount; i++)
        {
            var offset = checked((nuint)view._modePreference.ModeStructSizeHint * i);
            var modePtr = IntPtr.Add(modes, checked((int)offset));
            var width = lv_linux_drm_mode_get_horizontal_resolution_native(modePtr);
            var height = lv_linux_drm_mode_get_vertical_resolution_native(modePtr);
            var refresh = lv_linux_drm_mode_get_refresh_rate_native(modePtr);
            var isPreferred = lv_linux_drm_mode_is_preferred_native(modePtr);

            var score = view._modePreference.Score(width, height, refresh, isPreferred);
            if (score > preferredScore)
            {
                preferredScore = score;
                preferredIndex = i;
            }
        }

        return preferredIndex;
    }

    public readonly record struct DrmModePreference(int? Width, int? Height, int? RefreshRate, bool PreferNative)
    {
        public static DrmModePreference Default => new(null, null, null, false);

        public bool HasCustomSelector => Width is not null || Height is not null || RefreshRate is not null || PreferNative;

        public int ModeStructSizeHint => 128;

        public long Score(int width, int height, int refreshRate, bool isPreferred)
        {
            long score = 0;

            if (PreferNative && isPreferred)
            {
                score += 1_000_000;
            }

            if (Width is int targetWidth)
            {
                score -= Math.Abs(targetWidth - width) * 10L;
            }

            if (Height is int targetHeight)
            {
                score -= Math.Abs(targetHeight - height) * 10L;
            }

            if (RefreshRate is int targetRefresh)
            {
                score -= Math.Abs(targetRefresh - refreshRate);
            }

            score += width * height;
            score += refreshRate;
            return score;
        }

        public override string ToString()
        {
            return $"{Width?.ToString() ?? "*"}x{Height?.ToString() ?? "*"}@{RefreshRate?.ToString() ?? "*"}, PreferNative={PreferNative}";
        }
    }
}