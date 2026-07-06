using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;
using Terminalia.Editor.Styling.Layout;

namespace Terminalia.Editor.Shell;

public sealed partial class EditorShell
{
    private void BuildLayout()
    {
        _canvasScroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = EditorColors.Workspace,
            Padding = new Padding(EditorLayout.CanvasPadding)
        };
        EditorControls.ApplyDarkScrollBars(_canvasScroll);
        _canvasScroll.Controls.Add(Canvas);
        _canvasScroll.Resize += (_, _) => CenterCanvas(_canvasScroll);
        _canvasScroll.MouseWheel += (_, e) => Canvas.CellSize += e.Delta > 0 ? 4 : -4;
        Canvas.SizeChanged += (_, _) => CenterCanvas(_canvasScroll);

        var contentHost = new Panel { Dock = DockStyle.Fill };
        contentHost.Controls.Add(Runtime);
        contentHost.Controls.Add(_canvasScroll);
        contentHost.Controls.Add(TabBar);

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = EditorColors.AppBackground
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, EditorLayout.CommandBarWidth));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainLayout.Controls.Add(CommandBar, 0, 0);
        mainLayout.Controls.Add(contentHost, 1, 0);

        _mainHost.Dock = DockStyle.Fill;
        _mainHost.BackColor = EditorColors.AppBackground;
        _mainHost.Controls.Add(mainLayout);
        _mainHost.Controls.Add(Inspector);
        _mainHost.Resize += (_, _) => PlaceInspector();

        var shellLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = EditorColors.AppBackground
        };
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shellLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, EditorLayout.StatusBarHeight));
        shellLayout.Controls.Add(_mainHost, 0, 0);
        shellLayout.Controls.Add(StatusLabel, 0, 1);

        Controls.Add(shellLayout);
    }

    private static Label BuildStatusLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Height = EditorLayout.StatusBarHeight,
            Font = EditorFonts.SmallFont,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 0, 0),
            BackColor = EditorColors.StatusBar,
            ForeColor = EditorColors.SecondaryText
        };
    }
}
