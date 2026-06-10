using Terminalia.Core.Abstractions.Rendering;

namespace Terminalia.Core.Menus;

public static class MenuView
{
    public static void Draw(IRenderer renderer, MenuDefinition definition, int selectedIndex)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(definition);

        var totalLines = definition.Items.Count + 2;
        var startY = Math.Max(0, (renderer.Height - totalLines) / 2);

        DrawLine(renderer, startY, definition.Heading.ToUpperInvariant());

        for (var i = 0; i < definition.Items.Count; i++)
        {
            var item = definition.Items[i];
            var text = i == selectedIndex ? $"> {item.Label}" : $"  {item.Label}";
            DrawLine(renderer, startY + 2 + i, text);
        }
    }

    private static void DrawLine(IRenderer renderer, int y, string text)
    {
        if (y < 0 || y >= renderer.Height)
        {
            return;
        }

        renderer.DrawTextCentered(y, text);
    }
}
