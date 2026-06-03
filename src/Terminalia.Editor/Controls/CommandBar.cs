namespace Terminalia.Editor;

public sealed class CommandBar : UserControl
{
    private const int ButtonHeight = 30;
    private const int ButtonGap = 8;
    private readonly Label _title;
    private readonly List<Button> _buttons = [];

    public event EventHandler? NewRequested;
    public event EventHandler? OpenRequested;
    public event EventHandler? SaveRequested;
    public event EventHandler? SaveAsRequested;
    public event EventHandler? OpenSampleRequested;
    public event EventHandler? SaveSampleRequested;
    public event EventHandler? RunExternalRequested;

    public CommandBar()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorTheme.Surface;
        Padding = new Padding(14, 8, 14, 8);
        MinimumSize = new Size(0, 76);

        _title = new Label
        {
            Text = "Terminalia Editor",
            Width = 170,
            Height = ButtonHeight,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
            ForeColor = EditorTheme.PrimaryText
        };

        AddButton("New", () => NewRequested?.Invoke(this, EventArgs.Empty));
        AddButton("Open", () => OpenRequested?.Invoke(this, EventArgs.Empty));
        AddButton("Save", () => SaveRequested?.Invoke(this, EventArgs.Empty));
        AddButton("Save As", () => SaveAsRequested?.Invoke(this, EventArgs.Empty));
        AddButton("Open Sample", () => OpenSampleRequested?.Invoke(this, EventArgs.Empty), 112);
        AddButton("Save Sample", () => SaveSampleRequested?.Invoke(this, EventArgs.Empty), 112);
        AddButton("Run External", () => RunExternalRequested?.Invoke(this, EventArgs.Empty), 112);

        Controls.Add(_title);
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);

        if (_title is null || _buttons is null)
        {
            return;
        }

        var left = Padding.Left;
        var top = Padding.Top;
        _title.Bounds = new Rectangle(left, top, _title.Width, ButtonHeight);

        var x = _title.Right + 16;
        var y = top;
        var rightLimit = Math.Max(x, ClientSize.Width - Padding.Right);

        foreach (var button in _buttons)
        {
            if (x + button.Width > rightLimit && x > _title.Right + 16)
            {
                x = _title.Right + 16;
                y += ButtonHeight + 6;
            }

            button.Bounds = new Rectangle(x, y, button.Width, ButtonHeight);
            x += button.Width + ButtonGap;
        }
    }

    private void AddButton(string text, Action action, int width = 88)
    {
        var button = EditorTheme.CreateButton(text);
        button.Width = width;
        button.Click += (_, _) => action();
        _buttons.Add(button);
        Controls.Add(button);
    }
}
