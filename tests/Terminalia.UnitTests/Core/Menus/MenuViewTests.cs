using Terminalia.Core.Abstractions.Rendering;
using Terminalia.Core.Menus;

namespace Terminalia.UnitTests.Core.Menus;

public sealed class MenuViewTests
{
    [Fact]
    public void MenuView_Should_Draw_Heading_Centered_And_Uppercase()
    {
        var renderer = new FakeRenderer();
        var menu = CreateMenu();

        MenuView.Draw(renderer, menu, selectedIndex: 0);

        var heading = renderer.Texts.First();
        Assert.Equal("MY GAME", heading.Text);
        Assert.InRange(heading.Y, 0, renderer.Height - 1);
    }

    [Fact]
    public void MenuView_Should_Draw_All_Item_Labels()
    {
        var renderer = new FakeRenderer();
        var menu = CreateMenu();

        MenuView.Draw(renderer, menu, selectedIndex: 0);

        Assert.Contains(renderer.Texts, t => t.Text.Contains("start"));
        Assert.Contains(renderer.Texts, t => t.Text.Contains("quit"));
    }

    [Fact]
    public void MenuView_Should_Mark_Only_Selected_Item()
    {
        var renderer = new FakeRenderer();
        var menu = CreateMenu();

        MenuView.Draw(renderer, menu, selectedIndex: 1);

        var marked = Assert.Single(renderer.Texts, t => t.Text.StartsWith("> "));
        Assert.Equal("> quit", marked.Text);
    }

    private static MenuDefinition CreateMenu()
    {
        return new MenuDefinition("My Game", [
            new MenuItemDefinition("start", MenuAction.StartGame),
            new MenuItemDefinition("quit", MenuAction.QuitGame)
        ]);
    }

    private sealed class FakeRenderer : IRenderer
    {
        public int Width => 40;

        public int Height => 20;

        public List<(int Y, string Text)> Texts { get; } = [];

        public void Clear()
        {
        }

        public void Draw(int x, int y, char character)
        {
        }

        public void DrawText(int x, int y, string text)
        {
        }

        public void DrawTextCentered(int y, string text)
        {
            Texts.Add((y, text));
        }

        public void Present()
        {
        }
    }
}
