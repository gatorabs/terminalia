using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Drawing;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Shared;

public sealed class LoadingOverlay : UserControl
{
    private readonly System.Windows.Forms.Timer _timer = new();
    private int _frame;
    private string _message = "Loading...";

    public LoadingOverlay()
    {
        Dock = DockStyle.Fill;
        Visible = false;
        TabStop = false;
        BackColor = Color.Transparent;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);

        _timer.Interval = 80;
        _timer.Tick += (_, _) =>
        {
            _frame++;
            Invalidate();
        };
    }

    public void ShowLoading(string message)
    {
        _message = message;
        _frame = 0;
        Visible = true;
        Enabled = true;
        BringToFront();
        _timer.Start();
        Invalidate();
    }

    public void HideLoading()
    {
        _timer.Stop();
        Visible = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var scrim = new SolidBrush(EditorColors.OverlayScrim);
        e.Graphics.FillRectangle(scrim, ClientRectangle);

        var cardSize = new Size(300, 112);
        var cardBounds = new Rectangle(
            (Width - cardSize.Width) / 2,
            (Height - cardSize.Height) / 2,
            cardSize.Width,
            cardSize.Height);

        using var cardPath = EditorDrawing.CreateRoundedPath(cardBounds, 14);
        using var cardFill = new SolidBrush(EditorColors.LoadingCard);
        using var cardBorder = new Pen(EditorColors.Border);
        e.Graphics.FillPath(cardFill, cardPath);
        e.Graphics.DrawPath(cardBorder, cardPath);

        DrawSpinner(e.Graphics, new Point(cardBounds.Left + 42, cardBounds.Top + 56));

        var textBounds = new Rectangle(cardBounds.Left + 76, cardBounds.Top + 30, cardBounds.Width - 100, 52);
        TextRenderer.DrawText(
            e.Graphics,
            _message,
            EditorFonts.SectionFont,
            textBounds,
            EditorColors.PrimaryText,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void DrawSpinner(Graphics graphics, Point center)
    {
        const int radius = 15;
        for (var i = 0; i < 8; i++)
        {
            var angle = (Math.PI * 2 / 8) * i;
            var alphaStep = (i - _frame) % 8;
            if (alphaStep < 0)
            {
                alphaStep += 8;
            }

            var alpha = 70 + (7 - alphaStep) * 22;
            var point = new PointF(
                center.X + (float)Math.Cos(angle) * radius,
                center.Y + (float)Math.Sin(angle) * radius);

            using var brush = new SolidBrush(EditorColors.WithAlpha(Math.Clamp(alpha, 70, 224), EditorColors.Accent));
            graphics.FillEllipse(brush, point.X - 3, point.Y - 3, 6, 6);
        }
    }
}
