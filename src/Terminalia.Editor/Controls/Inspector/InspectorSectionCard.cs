using Terminalia.Editor.Controls.Shared;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Inspector;

internal static class InspectorSectionCard
{
    public static Panel Create(string title, Control content, int height)
    {
        var section = new CardPanel
        {
            Dock = DockStyle.Top,
            Height = height,
            FillColor = EditorColors.Section,
            BorderColor = EditorColors.BorderSoft,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 10)
        };

        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Top,
            Height = 24,
            Font = EditorFonts.SectionFont,
            ForeColor = EditorColors.SecondaryText,
            BackColor = EditorColors.Section,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var separator = new Panel
        {
            Dock = DockStyle.Top,
            Height = 1,
            BackColor = EditorColors.BorderSoft
        };

        var contentArea = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = EditorColors.Section,
            Padding = new Padding(0, 6, 0, 0)
        };
        EditorControls.ApplyDarkScrollBars(contentArea);

        content.Dock = DockStyle.Fill;
        contentArea.Controls.Add(content);

        section.Controls.Add(contentArea);
        section.Controls.Add(separator);
        section.Controls.Add(titleLabel);

        return section;
    }
}
