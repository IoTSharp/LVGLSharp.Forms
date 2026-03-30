using LVGLSharp.Interop;

namespace LVGLSharp.Runtime.Linux;

internal static unsafe class LinuxRuntimeFontHelper
{
    internal readonly struct LinuxRuntimeFontInitialization
    {
        internal LinuxRuntimeFontInitialization(
            lv_font_t* fallbackFont,
            SixLaborsFontManager? fontManager,
            string? resolvedSystemFontPath,
            string fontDiagnosticSummary,
            string glyphDiagnosticSummary,
            lv_style_t* defaultFontStyle)
        {
            FallbackFont = fallbackFont;
            FontManager = fontManager;
            ResolvedSystemFontPath = resolvedSystemFontPath;
            FontDiagnosticSummary = fontDiagnosticSummary;
            GlyphDiagnosticSummary = glyphDiagnosticSummary;
            DefaultFontStyle = defaultFontStyle;
        }

        internal lv_font_t* FallbackFont { get; }

        internal SixLaborsFontManager? FontManager { get; }

        internal string? ResolvedSystemFontPath { get; }

        internal string FontDiagnosticSummary { get; }

        internal string GlyphDiagnosticSummary { get; }

        internal lv_style_t* DefaultFontStyle { get; }

        internal void ApplyTo(
            ref lv_font_t* fallbackFont,
            ref SixLaborsFontManager? fontManager,
            ref lv_style_t* defaultFontStyle)
        {
            fallbackFont = FallbackFont;
            fontManager = FontManager;
            defaultFontStyle = DefaultFontStyle;
        }

        internal void ApplyDiagnosticsTo(
            ref lv_font_t* fallbackFont,
            ref SixLaborsFontManager? fontManager,
            ref string? fontDiagnosticSummary,
            ref string? glyphDiagnosticSummary,
            ref lv_style_t* defaultFontStyle)
        {
            ApplyTo(ref fallbackFont, ref fontManager, ref defaultFontStyle);
            fontDiagnosticSummary = FontDiagnosticSummary;
            glyphDiagnosticSummary = GlyphDiagnosticSummary;
        }

        internal void ApplyPathAndDiagnosticTo(
            ref lv_font_t* fallbackFont,
            ref SixLaborsFontManager? fontManager,
            ref string? resolvedSystemFontPath,
            ref string? fontDiagnosticSummary,
            ref lv_style_t* defaultFontStyle)
        {
            ApplyTo(ref fallbackFont, ref fontManager, ref defaultFontStyle);
            resolvedSystemFontPath = ResolvedSystemFontPath;
            fontDiagnosticSummary = FontDiagnosticSummary;
        }

        internal void ApplyFullTo(
            ref lv_font_t* fallbackFont,
            ref SixLaborsFontManager? fontManager,
            ref string? resolvedSystemFontPath,
            ref string? fontDiagnosticSummary,
            ref string? glyphDiagnosticSummary,
            ref lv_style_t* defaultFontStyle)
        {
            ApplyTo(ref fallbackFont, ref fontManager, ref defaultFontStyle);
            resolvedSystemFontPath = ResolvedSystemFontPath;
            fontDiagnosticSummary = FontDiagnosticSummary;
            glyphDiagnosticSummary = GlyphDiagnosticSummary;
        }
    }

    internal static LinuxRuntimeFontInitialization InitializeRuntimeFont(
        lv_obj_t* root,
        float dpi,
        bool disableManagedFont = false,
        float size = 12)
    {
        var fallbackFont = LvglFontHelper.GetEffectiveTextFont(root, lv_part_t.LV_PART_MAIN);
        var fontManager = TryApplySystemFont(
            root,
            dpi,
            fallbackFont,
            out var resolvedSystemFontPath,
            out var fontDiagnosticSummary,
            out var glyphDiagnosticSummary,
            out var defaultFontStyle,
            disableManagedFont,
            size);

        return new LinuxRuntimeFontInitialization(
            fallbackFont,
            fontManager,
            resolvedSystemFontPath,
            fontDiagnosticSummary,
            glyphDiagnosticSummary,
            defaultFontStyle);
    }

    internal static SixLaborsFontManager? TryApplySystemFont(
        lv_obj_t* root,
        float dpi,
        lv_font_t* fallback,
        out string? resolvedSystemFontPath,
        out string fontDiagnosticSummary,
        out string glyphDiagnosticSummary,
        out lv_style_t* defaultFontStyle,
        bool disableManagedFont = false,
        float size = 12)
    {
        fontDiagnosticSummary = LinuxSystemFontResolver.GetFontPathDiagnosticSummary();
        glyphDiagnosticSummary = LinuxSystemFontResolver.GetGlyphDiagnosticSummary();
        resolvedSystemFontPath = disableManagedFont ? null : LinuxSystemFontResolver.TryResolveFontPath();
        defaultFontStyle = null;

        if (string.IsNullOrWhiteSpace(resolvedSystemFontPath))
        {
            return null;
        }

        return LvglFontHelper.ApplyManagedFont(
            root,
            resolvedSystemFontPath,
            size,
            dpi,
            fallback,
            out _,
            out defaultFontStyle);
    }

    internal static void ReleaseRuntimeFont(
        ref lv_font_t* fallbackFont,
        ref SixLaborsFontManager? fontManager,
        ref lv_style_t* defaultFontStyle)
    {
        LvglRuntimeFontRegistry.ClearActiveTextFont();
        fontManager?.Dispose();
        fontManager = null;
        fallbackFont = null;
        defaultFontStyle = null;
    }

    internal static void ReleaseRuntimeFontDiagnostics(
        ref lv_font_t* fallbackFont,
        ref SixLaborsFontManager? fontManager,
        ref string? fontDiagnosticSummary,
        ref string? glyphDiagnosticSummary,
        ref lv_style_t* defaultFontStyle)
    {
        ReleaseRuntimeFont(ref fallbackFont, ref fontManager, ref defaultFontStyle);
        fontDiagnosticSummary = null;
        glyphDiagnosticSummary = null;
    }

    internal static void ReleaseRuntimeFontPathAndDiagnostic(
        ref lv_font_t* fallbackFont,
        ref SixLaborsFontManager? fontManager,
        ref string? resolvedSystemFontPath,
        ref string? fontDiagnosticSummary,
        ref lv_style_t* defaultFontStyle)
    {
        ReleaseRuntimeFont(ref fallbackFont, ref fontManager, ref defaultFontStyle);
        resolvedSystemFontPath = null;
        fontDiagnosticSummary = null;
    }

    internal static void ReleaseRuntimeFontFull(
        ref lv_font_t* fallbackFont,
        ref SixLaborsFontManager? fontManager,
        ref string? resolvedSystemFontPath,
        ref string? fontDiagnosticSummary,
        ref string? glyphDiagnosticSummary,
        ref lv_style_t* defaultFontStyle)
    {
        ReleaseRuntimeFont(ref fallbackFont, ref fontManager, ref defaultFontStyle);
        resolvedSystemFontPath = null;
        fontDiagnosticSummary = null;
        glyphDiagnosticSummary = null;
    }
}
