using Terminalia.Editor.Styling.Colors;

namespace Terminalia.Editor.Screens.StartPage;

public sealed class StartPage : UserControl
{
    private static readonly string[] Labels = ["new map", "open map"];

    private readonly TerminalMenu _menu;

    public StartPage()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.StartBackground;

        _menu = new TerminalMenu(Labels);
        _menu.ItemFired += (_, i) => Fire(i);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = EditorColors.StartBackground
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
        if (Visible)
        {
            _menu.Focus();
        }
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
}
