using Terminalia.Core.Menus;

namespace Terminalia.Core.Assets;

public sealed class GameDefinition
{
    public GameDefinition(string title, string startMap, MenuDefinition titleMenu, MenuDefinition pauseMenu)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(startMap);
        ArgumentNullException.ThrowIfNull(titleMenu);
        ArgumentNullException.ThrowIfNull(pauseMenu);

        Title = title;
        StartMap = startMap;
        TitleMenu = titleMenu;
        PauseMenu = pauseMenu;
    }

    public string Title { get; }

    public string StartMap { get; }

    public MenuDefinition TitleMenu { get; }

    public MenuDefinition PauseMenu { get; }

    public static GameDefinition CreateDefault(string startMap)
    {
        var titleMenu = new MenuDefinition("TERMINALIA", [
            new MenuItemDefinition("start", MenuAction.StartGame),
            new MenuItemDefinition("quit", MenuAction.QuitGame)
        ]);

        var pauseMenu = new MenuDefinition("paused", [
            new MenuItemDefinition("resume", MenuAction.ResumeGame),
            new MenuItemDefinition("back to title", MenuAction.ReturnToTitle),
            new MenuItemDefinition("quit", MenuAction.QuitGame)
        ]);

        return new GameDefinition("Terminalia Game", startMap, titleMenu, pauseMenu);
    }
}
