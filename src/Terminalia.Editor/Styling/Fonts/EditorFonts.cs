namespace Terminalia.Editor.Styling.Fonts;

internal static class EditorFonts
{
    public static readonly Font UiFont = new(FontFamily.GenericMonospace, 9, FontStyle.Regular);
    public static readonly Font SectionFont = new(FontFamily.GenericMonospace, 9, FontStyle.Bold);
    public static readonly Font SmallFont = new(FontFamily.GenericMonospace, 8, FontStyle.Regular);
    public static readonly Font TitleFont = new(FontFamily.GenericMonospace, 14, FontStyle.Bold);
    public static readonly Font BadgeFont = new(FontFamily.GenericMonospace, 7, FontStyle.Regular);
    public static readonly Font HintFont = new(FontFamily.GenericMonospace, 7.5f, FontStyle.Regular);
    public static readonly Font MenuFont = new(FontFamily.GenericMonospace, 12, FontStyle.Regular);
    public static readonly Font LogoFont = new(FontFamily.GenericMonospace, 22, FontStyle.Bold);
    public static readonly Font GlyphFont = new(FontFamily.GenericMonospace, 10, FontStyle.Bold);
    public static readonly Font GlyphSmallFont = new(FontFamily.GenericMonospace, 9, FontStyle.Bold);
    public static readonly Font CanvasFont = new(FontFamily.GenericMonospace, 16, FontStyle.Bold);

    public static Font MonospaceBold(float size) =>
        new(FontFamily.GenericMonospace, size, FontStyle.Bold);
}
