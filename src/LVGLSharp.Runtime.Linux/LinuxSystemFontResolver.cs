using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;

namespace LVGLSharp.Runtime.Linux;

internal static class LinuxSystemFontResolver
{
    private static readonly Lazy<FontFamily?> s_cachedFontFamily = new(TryResolveFontFamilyCore);

    private static readonly string[] s_appPreferredFamilyNames =
    [
        "Noto Sans CJK SC",
        "Noto Serif CJK SC",
        "Source Han Sans SC",
        "Source Han Sans CN",
        "WenQuanYi Zen Hei",
        "Droid Sans Fallback",
        "AR PL UMing CN",
        "AR PL UKai CN",
        "Microsoft YaHei",
        "SimHei",
        "Noto Sans Mono CJK SC",
        "Noto Sans CJK JP",
        "DejaVu Sans",
        "Liberation Sans",
        "FreeSans",
    ];

    private static readonly string s_fontCoverageSample =
        "PictureBox演示程序图像路径输入加载显示模式抗锯齿左旋右旋放大缩小重置就绪串口刷新波特率打开关闭发送接收文本按钮菜单设备文件路径";

    private static readonly int s_requiredCoverage = CountCoverageSampleRunes();

    internal static FontFamily? TryResolveFontFamily()
    {
        return s_cachedFontFamily.Value;
    }

    private static FontFamily? TryResolveFontFamilyCore()
    {
        var appPreferredFamily = TryResolveApplicationPreferredFamily();
        if (appPreferredFamily is not null)
        {
            return appPreferredFamily;
        }

        var desktopFontFamily = TryResolveDesktopPreferredFamily();
        if (desktopFontFamily is not null)
        {
            return desktopFontFamily;
        }

        return TryResolveFallbackFamily();
    }

    private static FontFamily? TryResolveApplicationPreferredFamily()
    {
        foreach (var familyName in s_appPreferredFamilyNames)
        {
            if (TryResolveNamedFamily(familyName, requireFullCoverage: true, out var family))
            {
                return family;
            }
        }

        return null;
    }

    private static FontFamily? TryResolveDesktopPreferredFamily()
    {
        foreach (var desktopFontSetting in EnumerateDesktopFontSettings())
        {
            var desktopFamilyName = NormalizeDesktopFontFamilyName(desktopFontSetting);
            if (string.IsNullOrWhiteSpace(desktopFamilyName))
            {
                continue;
            }

            if (TryResolveNamedFamily(desktopFamilyName, requireFullCoverage: true, out var family))
            {
                return family;
            }

            if (TryResolveFontconfigFamilyName(desktopFamilyName, out var fontconfigFamilyName) &&
                TryResolveNamedFamily(fontconfigFamilyName, requireFullCoverage: true, out family))
            {
                return family;
            }
        }

        return null;
    }

    private static FontFamily? TryResolveFallbackFamily()
    {
        HashSet<string> seenNames = new(StringComparer.OrdinalIgnoreCase);
        List<FontFamily> preferredFamilies = new();

        foreach (var preferredFamilyName in s_appPreferredFamilyNames)
        {
            if (SystemFonts.TryGet(preferredFamilyName, out var preferredFamily) && seenNames.Add(preferredFamily.Name))
            {
                preferredFamilies.Add(preferredFamily);
            }
        }

        var preferredMatch = TryResolveBestFamily(preferredFamilies);
        if (preferredMatch is not null)
        {
            return preferredMatch;
        }

        List<FontFamily> fallbackFamilies = new();
        foreach (var family in SystemFonts.Families)
        {
            if (seenNames.Add(family.Name))
            {
                fallbackFamilies.Add(family);
            }
        }

        return TryResolveBestFamily(fallbackFamilies);
    }

    private static bool TryResolveNamedFamily(string familyName, bool requireFullCoverage, out FontFamily family)
    {
        family = default;

        if (!SystemFonts.TryGet(familyName, out family))
        {
            return false;
        }

        if (!TryScoreFamily(family, out var score))
        {
            return false;
        }

        return !requireFullCoverage || score.Coverage >= s_requiredCoverage;
    }

    private static FontFamily? TryResolveBestFamily(IEnumerable<FontFamily> candidateFamilies)
    {
        FontFamily? bestFamily = null;
        FontCandidateScore bestScore = default;

        foreach (var family in candidateFamilies)
        {
            if (!TryScoreFamily(family, out var score))
            {
                continue;
            }

            if (bestFamily is null || score.CompareTo(bestScore) > 0)
            {
                bestFamily = family;
                bestScore = score;
            }
        }

        return bestScore.Coverage > 0 ? bestFamily : null;
    }

    private static bool TryScoreFamily(FontFamily family, out FontCandidateScore score)
    {
        score = default;

        try
        {
            var font = family.CreateFont(12);
            int glyphCoverage = 0;

            foreach (var rune in s_fontCoverageSample.EnumerateRunes())
            {
                if (Rune.IsWhiteSpace(rune))
                {
                    continue;
                }

                if (font.TryGetGlyphs(new CodePoint(rune.Value), out var glyphs) && glyphs.Count > 0)
                {
                    glyphCoverage++;
                }
            }

            int preferredRank = Array.FindIndex(
                s_appPreferredFamilyNames,
                preferred => string.Equals(preferred, family.Name, StringComparison.OrdinalIgnoreCase));

            score = new FontCandidateScore(
                glyphCoverage,
                preferredRank >= 0 ? s_appPreferredFamilyNames.Length - preferredRank : 0,
                IsSansFamily(family.Name) ? 1 : 0,
                string.Equals(family.Culture?.TwoLetterISOLanguageName, "zh", StringComparison.OrdinalIgnoreCase) ? 1 : 0);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsSansFamily(string familyName)
    {
        return familyName.Contains("Sans", StringComparison.OrdinalIgnoreCase) ||
               familyName.Contains("Hei", StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> EnumerateDesktopFontSettings()
    {
        var seenValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in EnumerateGSettingsFontNames())
        {
            if (!string.IsNullOrWhiteSpace(value) && seenValues.Add(value))
            {
                yield return value;
            }
        }

        if (TryReadXfceFontName(out var xfceFontName) &&
            !string.IsNullOrWhiteSpace(xfceFontName) &&
            seenValues.Add(xfceFontName))
        {
            yield return xfceFontName;
        }

        if (TryReadKdeFontName(out var kdeFontName) &&
            !string.IsNullOrWhiteSpace(kdeFontName) &&
            seenValues.Add(kdeFontName))
        {
            yield return kdeFontName;
        }

        if (TryReadGtkSettingsFontName(out var gtkFontName) &&
            !string.IsNullOrWhiteSpace(gtkFontName) &&
            seenValues.Add(gtkFontName))
        {
            yield return gtkFontName;
        }
    }

    private static IEnumerable<string> EnumerateGSettingsFontNames()
    {
        var gsettingsKeys = new (string Schema, string Key)[]
        {
            ("org.gnome.desktop.interface", "font-name"),
            ("org.gnome.desktop.interface", "document-font-name"),
            ("org.cinnamon.desktop.interface", "font-name"),
            ("org.mate.interface", "font-name"),
        };

        foreach (var (schema, key) in gsettingsKeys)
        {
            if (TryRunCommand("gsettings", ["get", schema, key], out var output) &&
                TryNormalizeCommandOutput(output, out var normalizedOutput))
            {
                yield return normalizedOutput;
            }
        }
    }

    private static bool TryReadXfceFontName(out string fontName)
    {
        if (TryRunCommand(
                "xfconf-query",
                ["--channel", "xsettings", "--property", "/Gtk/FontName"],
                out var output) &&
            TryNormalizeCommandOutput(output, out var normalizedOutput))
        {
            fontName = normalizedOutput;
            return true;
        }

        fontName = string.Empty;
        return false;
    }

    private static bool TryReadKdeFontName(out string fontName)
    {
        var kdeGlobalsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "kdeglobals");

        if (File.Exists(kdeGlobalsPath))
        {
            foreach (var line in File.ReadLines(kdeGlobalsPath))
            {
                if (!line.StartsWith("font=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = line["font=".Length..].Trim();
                var separatorIndex = value.IndexOf(',');
                fontName = separatorIndex >= 0 ? value[..separatorIndex].Trim() : value;
                return !string.IsNullOrWhiteSpace(fontName);
            }
        }

        fontName = string.Empty;
        return false;
    }

    private static bool TryReadGtkSettingsFontName(out string fontName)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidatePaths = new[]
        {
            Path.Combine(home, ".config", "gtk-4.0", "settings.ini"),
            Path.Combine(home, ".config", "gtk-3.0", "settings.ini"),
        };

        foreach (var path in candidatePaths)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            foreach (var line in File.ReadLines(path))
            {
                if (!line.StartsWith("gtk-font-name=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                fontName = line["gtk-font-name=".Length..].Trim();
                return !string.IsNullOrWhiteSpace(fontName);
            }
        }

        fontName = string.Empty;
        return false;
    }

    private static bool TryResolveFontconfigFamilyName(string familyName, out string resolvedFamilyName)
    {
        if (TryRunCommand("fc-match", ["-f", "%{family[0]}\n", familyName], out var output) &&
            TryNormalizeCommandOutput(output, out var normalizedOutput))
        {
            resolvedFamilyName = normalizedOutput;
            return true;
        }

        resolvedFamilyName = string.Empty;
        return false;
    }

    private static string NormalizeDesktopFontFamilyName(string value)
    {
        var normalized = value.Trim().Trim('\'', '"');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (normalized.Contains(','))
        {
            normalized = normalized[..normalized.IndexOf(',')].Trim();
        }

        var tokens = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        while (tokens.Count > 0 && IsFontSizeToken(tokens[^1]))
        {
            tokens.RemoveAt(tokens.Count - 1);
        }

        while (tokens.Count > 0 && IsFontStyleToken(tokens[^1]))
        {
            tokens.RemoveAt(tokens.Count - 1);
        }

        return string.Join(' ', tokens).Trim();
    }

    private static bool TryNormalizeCommandOutput(string output, out string normalizedOutput)
    {
        normalizedOutput = NormalizeDesktopFontFamilyName(output);
        return !string.IsNullOrWhiteSpace(normalizedOutput);
    }

    private static bool IsFontSizeToken(string token)
    {
        return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }

    private static bool IsFontStyleToken(string token)
    {
        return token.Equals("Regular", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Bold", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Italic", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Oblique", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Medium", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Light", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Thin", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Black", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Book", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("SemiBold", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Semibold", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("DemiLight", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("ExtraLight", StringComparison.OrdinalIgnoreCase) ||
               token.Equals("Condensed", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryRunCommand(string fileName, IReadOnlyList<string> arguments, out string output)
    {
        output = string.Empty;

        try
        {
            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            if (!process.Start())
            {
                return false;
            }

            output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(1000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static int CountCoverageSampleRunes()
    {
        int count = 0;
        foreach (var rune in s_fontCoverageSample.EnumerateRunes())
        {
            if (!Rune.IsWhiteSpace(rune))
            {
                count++;
            }
        }

        return count;
    }

    private readonly record struct FontCandidateScore(
        int Coverage,
        int PreferredRank,
        int SansBonus,
        int ChineseCultureBonus)
        : IComparable<FontCandidateScore>
    {
        public int CompareTo(FontCandidateScore other)
        {
            int coverageComparison = Coverage.CompareTo(other.Coverage);
            if (coverageComparison != 0)
            {
                return coverageComparison;
            }

            int preferredComparison = PreferredRank.CompareTo(other.PreferredRank);
            if (preferredComparison != 0)
            {
                return preferredComparison;
            }

            int sansComparison = SansBonus.CompareTo(other.SansBonus);
            if (sansComparison != 0)
            {
                return sansComparison;
            }

            return ChineseCultureBonus.CompareTo(other.ChineseCultureBonus);
        }
    }
}
