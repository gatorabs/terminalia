using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Screens.StartPage;

public sealed class TerminalMenu : Control
{
    private const int LogoH = 48;
    private const int SepH  = 28;
    private const int RowH  = 36;
    private const int NavH  = 24;
    private const int Gap   = 10;
    private const int W     = 300;

    private readonly string[] _labels;
    private int _selected = 0;
    private Rectangle[] _rowRects = [];
    private bool _cursorVisible = true;
    private readonly System.Windows.Forms.Timer _blinkTimer = new() { Interval = 530 };

    public event EventHandler<int>? ItemFired;

    public TerminalMenu() : this([])
    {
    }

    public TerminalMenu(string[] labels)
    {
        _labels = labels;
        BackColor = EditorColors.StartBackground;
        DoubleBuffered = true;
        TabStop = true;
        Cursor = Cursors.Default;

        var totalH = LogoH + SepH + _labels.Length * RowH + Gap + SepH + NavH;
        Size = new Size(W + 24, totalH);

        _blinkTimer.Tick += (_, _) => { _cursorVisible = !_cursorVisible; Invalidate(); };
        _blinkTimer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _blinkTimer.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Up or Keys.Down or Keys.Enter || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Up:
                if (_selected > 0)
                {
                    _selected--;
                    ResetBlink();
                }
                e.Handled = true;
                break;
            case Keys.Down:
                if (_selected < _labels.Length - 1)
                {
                    _selected++;
                    ResetBlink();
                }
                e.Handled = true;
                break;
            case Keys.Enter:
                ItemFired?.Invoke(this, _selected);
                e.Handled = true;
                break;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        for (var i = 0; i < _rowRects.Length; i++)
        {
            if (!_rowRects[i].Contains(e.Location))
            {
                continue;
            }
            Cursor = Cursors.Hand;
            if (_selected != i)
            {
                _selected = i;
                ResetBlink();
            }
            return;
        }
        Cursor = Cursors.Default;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Cursor = Cursors.Default;
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left)
        {
            ItemFired?.Invoke(this, _selected);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(EditorColors.StartBackground);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var y = 0;

        TextRenderer.DrawText(g, "terminalia", EditorFonts.LogoFont,
            new Rectangle(0, y, W, LogoH),
            EditorColors.StartText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        y += LogoH;

        DrawSep(g, 0, y + SepH / 2, W);
        y += SepH;

        _rowRects = new Rectangle[_labels.Length];
        for (var i = 0; i < _labels.Length; i++)
        {
            _rowRects[i] = new Rectangle(-4, y, W + 4, RowH);
            var sel = i == _selected;

            if (sel && _cursorVisible)
            {
                TextRenderer.DrawText(g, ">", EditorFonts.MenuFont,
                    new Rectangle(0, y, 16, RowH),
                    EditorColors.StartText,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }

            TextRenderer.DrawText(g, _labels[i], EditorFonts.MenuFont,
                new Rectangle(20, y, W - 20, RowH),
                sel ? EditorColors.StartText : EditorColors.StartTextDim,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            y += RowH;
        }

        y += Gap;

        DrawSep(g, 0, y + SepH / 2, W);
        y += SepH;

        TextRenderer.DrawText(g, "↑↓ navigate   ↵ open", EditorFonts.UiFont,
            new Rectangle(0, y, W, NavH),
            EditorColors.StartHint,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
    }

    private void ResetBlink()
    {
        _cursorVisible = true;
        _blinkTimer.Stop();
        _blinkTimer.Start();
        Invalidate();
    }

    private static void DrawSep(Graphics g, int x, int y, int width)
    {
        using var pen = new Pen(EditorColors.StartSeparator);
        g.DrawLine(pen, x, y, x + width, y);
    }
}
