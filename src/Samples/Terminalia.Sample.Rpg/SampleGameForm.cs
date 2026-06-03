namespace Terminalia.Sample.Rpg;

public sealed class SampleGameForm : Form
{
    public SampleGameForm()
    {
        Text = "Terminalia Sample RPG";
        MinimumSize = new Size(900, 520);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = SampleTheme.AppBackground;
        KeyPreview = true;

        var surface = new SampleGameSurface
        {
            Dock = DockStyle.Fill
        };

        Controls.Add(surface);
        KeyDown += (_, e) => surface.HandleKey(e.KeyCode);
    }
}
