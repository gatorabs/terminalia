using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Drawing;
using Terminalia.Editor.Styling.Fonts;
using Terminalia.Editor.Styling.Layout;

namespace Terminalia.Editor.Controls.CommandBar;

public sealed partial class CommandBar
{
    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(EditorColors.AppBackground);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rail = new Rectangle(EditorLayout.CommandPadding, EditorLayout.CommandPadding, Width - EditorLayout.CommandPadding * 2, Height - EditorLayout.CommandPadding * 2);

        using (var railPath = EditorDrawing.CreateRoundedPath(rail, 18))
        using (var railFill = new SolidBrush(EditorColors.Surface))
        using (var railBorder = new Pen(EditorColors.BorderSoft))
        {
            g.FillPath(railFill, railPath);
            g.DrawPath(railBorder, railPath);
        }

        DrawBrand(g);

        foreach (var item in _items)
        {
            if (item.IsSeparator)
            {
                PaintSeparator(g, item);
            }
            else
            {
                PaintButton(g, item);
            }
        }
    }

    private void DrawBrand(Graphics g)
    {
        TextRenderer.DrawText(g, ">_", EditorFonts.TitleFont,
            new Rectangle(0, 38, Width, 24), EditorColors.PrimaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(g, "TERM", EditorFonts.BadgeFont,
            new Rectangle(0, 62, Width, 16), EditorColors.SecondaryText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    private static void PaintSeparator(Graphics g, CommandBarItem item)
    {
        using var pen = new Pen(EditorColors.BorderSoft);
        var midY = item.Bounds.Top + item.Bounds.Height / 2;
        g.DrawLine(pen, item.Bounds.Left + 10, midY, item.Bounds.Right - 10, midY);
    }

    private void PaintButton(Graphics g, CommandBarItem item)
    {
        var b = item.Bounds;
        b.Inflate(-1, -1);
        if (item == _pressItem)
        {
            b.Offset(0, 1);
        }

        using var path = EditorDrawing.CreateRoundedPath(b, 12);
        using var fill = new SolidBrush(item.BackColor);
        var borderColor = item.IsActive
            ? EditorColors.SelectAccent
            : item.IsPrimary ? EditorColors.PrimaryText : EditorColors.BorderSoft;
        using var border = new Pen(borderColor, item.IsActive ? 2f : 1f);
        g.FillPath(fill, path);

        if (item.Pulse > 0.01)
        {
            using var pulseBrush = new SolidBrush(EditorColors.WhiteOverlay((int)(EditorColors.CommandPulseAlpha * item.Pulse)));
            g.FillPath(pulseBrush, path);
        }

        g.DrawPath(border, path);

        var iconFont = item.Glyph.Length > 2 ? EditorFonts.GlyphSmallFont : EditorFonts.GlyphFont;
        var glyphColor = item.IsActive
            ? EditorColors.SelectAccent
            : item.IsPrimary ? EditorColors.OnAccent : EditorColors.PrimaryText;
        TextRenderer.DrawText(g, item.Glyph, iconFont, b, glyphColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    private void DrawToolTip(object? sender, DrawToolTipEventArgs e)
    {
        using var fill = new SolidBrush(EditorColors.Surface);
        using var border = new Pen(EditorColors.Border);
        e.Graphics.FillRectangle(fill, e.Bounds);
        e.Graphics.DrawRectangle(border, new Rectangle(0, 0, e.Bounds.Width - 1, e.Bounds.Height - 1));
        TextRenderer.DrawText(e.Graphics, e.ToolTipText, EditorFonts.UiFont, e.Bounds,
            EditorColors.PrimaryText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
