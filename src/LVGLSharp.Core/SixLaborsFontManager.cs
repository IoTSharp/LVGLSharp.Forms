using LVGLSharp.Interop;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LVGLSharp
{
    public class GlyphCacheItem
    {
        public lv_font_glyph_dsc_t Dsc;
        public IntPtr Bitmap;
        public FontRectangle Bounds;
    }

    public unsafe class SixLaborsFontManager : IDisposable
    {
        private Font _font = null!;
        private float _lineHeight;
        private lv_font_t* _nativeLvFont;
        private HashSet<uint> _fallbackFOntUnicodeLetterRange = null!;
        private Configuration _configuration = null!;
        private float _dpi;

        private static readonly Dictionary<IntPtr, SixLaborsFontManager> s_fontManagers = new();
        private static readonly object s_lock = new();
        private static readonly bool s_traceGlyphs =
            string.Equals(Environment.GetEnvironmentVariable("LVGLSHARP_TRACE_GLYPHS"), "1", StringComparison.Ordinal);

        private readonly Dictionary<uint, GlyphCacheItem> _glyphCache = new();


        public SixLaborsFontManager(string fontPath, float size, float dpi = 72f, lv_font_t* fallback = null, HashSet<uint>? fallbackFOntUnicodeLetterRange = null)
        {
            FontCollection collection = new();
            FontFamily family = collection.Add(fontPath);
            var font = family.CreateFont(size, FontStyle.Regular);
            Init(font, dpi, fallback, fallbackFOntUnicodeLetterRange);
        }

        public SixLaborsFontManager(Font font, float dpi = 72f, lv_font_t* fallback = null, HashSet<uint>? fallbackFOntUnicodeLetterRange = null)
        {
            Init(font, dpi, fallback, fallbackFOntUnicodeLetterRange);
        }

        public SixLaborsFontManager(FontFamily fontFamily, float size, float dpi = 72f, lv_font_t* fallback = null, HashSet<uint>? fallbackFOntUnicodeLetterRange = null)
        {
            Init(new Font(fontFamily, size), dpi, fallback, fallbackFOntUnicodeLetterRange);
        }

        private void Init(Font font, float dpi = 72f, lv_font_t* fallback = null, HashSet<uint>? fallbackFOntUnicodeLetterRange = null)
        {
            Configuration.Default.MemoryAllocator = MemoryAllocator.Create(new MemoryAllocatorOptions()
            {
                MaximumPoolSizeMegabytes = 8,
                AllocationLimitMegabytes = 8
            });

            _dpi = dpi;

            _fallbackFOntUnicodeLetterRange = fallbackFOntUnicodeLetterRange ?? new HashSet<uint>();

            _font = font;

            _nativeLvFont = (lv_font_t*)NativeMemory.Alloc((nuint)sizeof(lv_font_t));
            NativeMemory.Clear(_nativeLvFont, (nuint)sizeof(lv_font_t));

            _configuration = Configuration.Default.Clone();
            _configuration.PreferContiguousImageBuffers = true;

            float scale = _font.Size / _font.FontMetrics.UnitsPerEm;
            var horizontalMetrics = _font.FontMetrics.HorizontalMetrics;
            float lineHeightPx = horizontalMetrics.LineHeight * scale;

            _lineHeight = lineHeightPx;

            _nativeLvFont->get_glyph_dsc = &GetGlyphDsc;
            _nativeLvFont->get_glyph_bitmap = &GetGlyphBitmap;
            _nativeLvFont->release_glyph = &ReleaseGlyph;
            _nativeLvFont->line_height = (int)Math.Round(lineHeightPx);
            _nativeLvFont->base_line = 0;
            _nativeLvFont->underline_position = (sbyte)Math.Round(_font.FontMetrics.UnderlinePosition * scale);
            _nativeLvFont->underline_thickness = (sbyte)Math.Round(_font.FontMetrics.UnderlineThickness * scale);
            _nativeLvFont->subpx = 0;
            _nativeLvFont->kerning = 0;
            _nativeLvFont->static_bitmap = 0;
            _nativeLvFont->fallback = fallback;
            _nativeLvFont->dsc = _nativeLvFont;

            lock (s_lock)
            {
                s_fontManagers[(IntPtr)_nativeLvFont] = this;
            }
        }

        public lv_font_t* GetLvFontPtr()
        {
            return _nativeLvFont;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static c_bool1 GetGlyphDsc(lv_font_t* font, lv_font_glyph_dsc_t* dsc_out, uint unicode_letter, uint unicode_letter_next)
        {
            try
            {
                if (!TryGetManager(font, out var manager))
                {
                    return false;
                }

                if (s_traceGlyphs)
                {
                    Console.Error.WriteLine(
                        FormattableString.Invariant(
                            $"glyph-req U+{unicode_letter:X4} '{FormatGlyph(unicode_letter)}' next=U+{unicode_letter_next:X4}"));
                }

                if (manager._fallbackFOntUnicodeLetterRange.Contains(unicode_letter))
                {
                    if (s_traceGlyphs)
                    {
                        Console.Error.WriteLine(FormattableString.Invariant($"glyph-fallback-range U+{unicode_letter:X4}"));
                    }
                    return false;
                }

                if (manager._glyphCache.TryGetValue(unicode_letter, out var cacheItem))
                {
                    *dsc_out = cacheItem.Dsc;
                    if (s_traceGlyphs)
                    {
                        Console.Error.WriteLine(
                            FormattableString.Invariant(
                                $"glyph-cache U+{unicode_letter:X4} adv={cacheItem.Dsc.adv_w} box={cacheItem.Dsc.box_w}x{cacheItem.Dsc.box_h} ofs=({cacheItem.Dsc.ofs_x},{cacheItem.Dsc.ofs_y})"));
                    }
                    return true;
                }

                if (!Rune.TryCreate(unicode_letter, out _))
                {
                    if (s_traceGlyphs)
                    {
                        Console.Error.WriteLine(FormattableString.Invariant($"glyph-invalid-codepoint U+{unicode_letter:X4}"));
                    }
                    return false;
                }

                var slFont = manager._font;
                if (!slFont.TryGetGlyphs(new CodePoint(unicode_letter), out var glyphs) || glyphs.Count == 0)
                {
                    if (s_traceGlyphs)
                    {
                        Console.Error.WriteLine(FormattableString.Invariant($"glyph-miss U+{unicode_letter:X4}"));
                    }
                    return false;
                }

                var glyph = glyphs[0];
                var glyphMetrics = glyph.GlyphMetrics;
                float scale = slFont.Size / slFont.FontMetrics.UnitsPerEm;
                FontRectangle bbox = glyph.BoundingBox(GlyphLayoutMode.Horizontal, Vector2.Zero, manager._dpi);

                int boxWidth = Math.Max(0, (int)Math.Ceiling(bbox.Width));
                int boxHeight = Math.Max(0, (int)Math.Ceiling(bbox.Height));
                int ofsX = (int)Math.Floor(bbox.Left);
                int ofsY = boxHeight == 0 ? 0 : (int)Math.Round((-bbox.Top) - boxHeight);

                int advanceWidth = (int)Math.Round(glyphMetrics.AdvanceWidth * scale * manager._dpi / 72f);
                advanceWidth = Math.Clamp(advanceWidth, 0, ushort.MaxValue);

                var dsc = new lv_font_glyph_dsc_t
                {
                    adv_w = (ushort)advanceWidth,
                    box_w = (ushort)Math.Min(boxWidth, ushort.MaxValue),
                    box_h = (ushort)Math.Min(boxHeight, ushort.MaxValue),
                    ofs_x = (short)Math.Clamp(ofsX, short.MinValue, short.MaxValue),
                    ofs_y = (short)Math.Clamp(ofsY, short.MinValue, short.MaxValue),
                    stride = (ushort)lv_draw_buf_width_to_stride((uint)Math.Min(boxWidth, ushort.MaxValue), lv_color_format_t.LV_COLOR_FORMAT_A8),
                    format = lv_font_glyph_format_t.LV_FONT_GLYPH_FORMAT_A8,
                    is_placeholder = 0
                };
                dsc.gid.index = unicode_letter;

                *dsc_out = dsc;
                manager._glyphCache[unicode_letter] = new GlyphCacheItem
                {
                    Dsc = dsc,
                    Bitmap = IntPtr.Zero,
                    Bounds = bbox
                };

                if (s_traceGlyphs)
                {
                    Console.Error.WriteLine(
                        FormattableString.Invariant(
                            $"glyph U+{unicode_letter:X4} '{FormatGlyph(unicode_letter)}' adv={dsc.adv_w} box={dsc.box_w}x{dsc.box_h} ofs=({dsc.ofs_x},{dsc.ofs_y}) bbox=({bbox.Left:F2},{bbox.Top:F2},{bbox.Width:F2},{bbox.Height:F2})"));
                }

                return true;
            }
            catch (Exception ex)
            {
                if (s_traceGlyphs)
                {
                    Console.Error.WriteLine(
                        FormattableString.Invariant(
                            $"glyph-ex U+{unicode_letter:X4} '{FormatGlyph(unicode_letter)}' {ex.GetType().Name}: {ex.Message}"));
                }
                return false;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static void* GetGlyphBitmap(lv_font_glyph_dsc_t* dsc, lv_draw_buf_t* draw_buf)
        {
            try
            {
                if (dsc->resolved_font == null || dsc->box_w <= 0 || dsc->box_h <= 0) return null;
                if (!TryGetManager(dsc->resolved_font, out var manager)) return null;

                var unicodeCodePoint = dsc->gid.index;
                var slFont = manager._font;

                if (!manager._glyphCache.TryGetValue(unicodeCodePoint, out var cacheItem))
                    return null;

                string characterToDraw = char.ConvertFromUtf32((int)unicodeCodePoint);
                using var image = new Image<A8>(manager._configuration, dsc->box_w, dsc->box_h);
                image.Mutate(x => x.DrawText(new RichTextOptions(slFont)
                {
                    Origin = new PointF(-cacheItem.Bounds.Left, -cacheItem.Bounds.Top),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Dpi = manager._dpi
                }, characterToDraw, Color.Black));

                if (draw_buf == null || draw_buf->data == null)
                {
                    return null;
                }

                int rowStride = (int)draw_buf->header.stride;
                if (rowStride <= 0)
                {
                    rowStride = dsc->box_w;
                }

                int requiredBufferSize = rowStride * dsc->box_h;
                if (draw_buf->data_size < requiredBufferSize)
                {
                    return null;
                }

                if (!image.DangerousTryGetSinglePixelMemory(out Memory<A8> sourcePixels))
                {
                    return null;
                }

                var sourceBytes = MemoryMarshal.AsBytes(sourcePixels.Span);
                var destinationBytes = new Span<byte>(draw_buf->data, requiredBufferSize);

                for (int y = 0; y < dsc->box_h; y++)
                {
                    int sourceOffset = y * dsc->box_w;
                    int destinationOffset = y * rowStride;
                    sourceBytes.Slice(sourceOffset, dsc->box_w).CopyTo(destinationBytes.Slice(destinationOffset, dsc->box_w));

                    // Clear the alignment padding bytes so stale alpha does not leak into the next glyph.
                    if (rowStride > dsc->box_w)
                    {
                        destinationBytes.Slice(destinationOffset + dsc->box_w, rowStride - dsc->box_w).Clear();
                    }
                }

                if (s_traceGlyphs)
                {
                    Console.Error.WriteLine(
                        FormattableString.Invariant(
                            $"bitmap U+{unicodeCodePoint:X4} '{FormatGlyph(unicodeCodePoint)}' req={dsc->box_w}x{dsc->box_h} stride={dsc->stride} draw_buf=({draw_buf->header.w}x{draw_buf->header.h} stride={draw_buf->header.stride} cf={draw_buf->header.cf} size={draw_buf->data_size})"));
                }

                return draw_buf;
            }
            catch (Exception ex)
            {
                if (s_traceGlyphs)
                {
                    Console.Error.WriteLine(
                        FormattableString.Invariant(
                            $"bitmap-ex U+{dsc->gid.index:X4} '{FormatGlyph(dsc->gid.index)}' {ex.GetType().Name}: {ex.Message}"));
                }

                return null;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static void ReleaseGlyph(lv_font_t* font, lv_font_glyph_dsc_t* dsc)
        {
            if (!TryGetManager(font, out var manager)) return;

            var letterId = dsc->gid.index;
            if (manager._glyphCache.TryGetValue(letterId, out var cacheItem))
            {
                if (cacheItem.Bitmap != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(cacheItem.Bitmap);
                    cacheItem.Bitmap = IntPtr.Zero;
                }
            }
        }

        private static bool TryGetManager(lv_font_t* font, out SixLaborsFontManager manager)
        {
            if (font == null)
            {
                manager = null!;
                return false;
            }

            lock (s_lock)
            {
                if (s_fontManagers.TryGetValue((IntPtr)font->dsc, out var found))
                {
                    manager = found;
                    return true;
                }
            }

            manager = null!;
            return false;
        }

        private static string FormatGlyph(uint unicodeLetter)
        {
            return Rune.TryCreate(unicodeLetter, out var rune)
                ? rune.ToString().Replace("\n", "\\n", StringComparison.Ordinal)
                : "?";
        }

        public void Dispose()
        {
            lock (s_lock)
            {
                if (_nativeLvFont == null) return;

                foreach (var cacheItem in _glyphCache.Values)
                {
                    if (cacheItem.Bitmap != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(cacheItem.Bitmap);
                    }
                }
                _glyphCache.Clear();

                s_fontManagers.Remove((IntPtr)_nativeLvFont);

                NativeMemory.Free(_nativeLvFont);
                _nativeLvFont = null;
            }
            GC.SuppressFinalize(this);
        }
    }
}
