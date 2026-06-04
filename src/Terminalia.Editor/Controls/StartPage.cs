namespace Terminalia.Editor;

public sealed class StartPage : UserControl
{
    private static readonly string[] Labels = ["new map", "open map"];

    private readonly TerminalMenu _menu;

    public StartPage()
    {
        Dock = DockStyle.Fill;
        BackColor = Color.Black;

        _menu = new TerminalMenu();
        _menu.ItemFired += (_, i) => Fire(i);

        // TableLayoutPanel keeps _menu anchored to upper-left with percentage margins
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = Color.Black
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 93f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 80f));
        layout.Controls.Add(_menu, 1, 1);

        Controls.Add(layout);
    }

    public event EventHandler? NewMapRequested;
    public event EventHandler? OpenMapRequested;

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible) _menu.Focus();
    }

    private void Fire(int index)
    {
        var handler = index switch
        {
            0 => NewMapRequested,
            1 => OpenMapRequested,
            _ => null
        };
        handler?.Invoke(this, EventArgs.Empty);
    }

    // Inner control — owns the terminal rendering and keyboard/mouse logic
    private sealed class TerminalMenu : Control
    {
        private static readonly Font LogoFont = new(FontFamily.GenericMonospace, 22, FontStyle.Bold);
        private static readonly Font CmdFont  = new(FontFamily.GenericMonospace, 12, FontStyle.Regular);
        private static readonly Font NavFont  = new(FontFamily.GenericMonospace, 9,  FontStyle.Regular);

        private const int LogoH = 48;
        private const int SepH  = 28;
        private const int RowH  = 36;
        private const int NavH  = 24;
        private const int Gap   = 10;
        private const int W     = 300;

        private int _selected = 0;
        private Rectangle[] _rowRects = [];
        private bool _cursorVisible = true;
        private readonly System.Windows.Forms.Timer _blinkTimer = new() { Interval = 530 };

        public event EventHandler<int>? ItemFired;

        public TerminalMenu()
        {
            BackColor = Color.Black;
            DoubleBuffered = true;
            TabStop = true;
            Cursor = Cursors.Default;

            var totalH = LogoH + SepH + Labels.Length * RowH + Gap + SepH + NavH;
            Size = new Size(W + 24, totalH);

            _blinkTimer.Tick += (_, _) => { _cursorVisible = !_cursorVisible; Invalidate(); };
            _blinkTimer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _blinkTimer.Dispose();
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
                    if (_selected > 0) { _selected--; ResetBlink(); }
                    e.Handled = true;
                    break;
                case Keys.Down:
                    if (_selected < Labels.Length - 1) { _selected++; ResetBlink(); }
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
                if (!_rowRects[i].Contains(e.Location)) continue;
                Cursor = Cursors.Hand;
                if (_selected != i) { _selected = i; ResetBlink(); }
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
            if (e.Button == MouseButtons.Left) ItemFired?.Invoke(this, _selected);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(Color.Black);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var y = 0;

            // Logo
            TextRenderer.DrawText(g, "terminalia", LogoFont,
                new Rectangle(0, y, W, LogoH),
                Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            y += LogoH;

            // Top separator
            DrawSep(g, 0, y + SepH / 2, W);
            y += SepH;

            // Rows
            _rowRects = new Rectangle[Labels.Length];
            for (var i = 0; i < Labels.Length; i++)
            {
                _rowRects[i] = new Rectangle(-4, y, W + 4, RowH);
                var sel = i == _selected;

                if (sel && _cursorVisible)
                {
                    TextRenderer.DrawText(g, ">", CmdFont,
                        new Rectangle(0, y, 16, RowH),
                        Color.White,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }

                TextRenderer.DrawText(g, Labels[i], CmdFont,
                    new Rectangle(20, y, W - 20, RowH),
                    sel ? Color.White : Color.FromArgb(80, 255, 255, 255),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                y += RowH;
            }

            y += Gap;

            // Bottom separator
            DrawSep(g, 0, y + SepH / 2, W);
            y += SepH;

            // Nav hint
            TextRenderer.DrawText(g, "↑↓ navigate   ↵ open", NavFont,
                new Rectangle(0, y, W, NavH),
                Color.FromArgb(38, 255, 255, 255),
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
            using var pen = new Pen(Color.FromArgb(22, 255, 255, 255));
            g.DrawLine(pen, x, y, x + width, y);
        }
    }
}
