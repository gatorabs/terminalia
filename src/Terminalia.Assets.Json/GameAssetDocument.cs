using System.Text.Json;
using Terminalia.Core.Assets;
using Terminalia.Core.Menus;

namespace Terminalia.Assets.Json;

public sealed class GameAssetDocument
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly Dictionary<string, MenuAction> ActionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["startGame"] = MenuAction.StartGame,
        ["resumeGame"] = MenuAction.ResumeGame,
        ["returnToTitle"] = MenuAction.ReturnToTitle,
        ["quitGame"] = MenuAction.QuitGame
    };

    public string Title { get; set; } = "Terminalia Game";

    public string StartMap { get; set; } = string.Empty;

    public GameMenuDto TitleMenu { get; set; } = new();

    public GameMenuDto PauseMenu { get; set; } = new();

    public static IReadOnlyCollection<string> KnownActions => ActionMap.Keys;

    public static GameAssetDocument CreateDefault(string startMap)
    {
        return new GameAssetDocument
        {
            Title = "Terminalia Game",
            StartMap = startMap,
            TitleMenu = new GameMenuDto
            {
                Heading = "TERMINALIA",
                Items =
                [
                    new GameMenuItemDto { Label = "start", Action = "startGame" },
                    new GameMenuItemDto { Label = "quit", Action = "quitGame" }
                ]
            },
            PauseMenu = new GameMenuDto
            {
                Heading = "paused",
                Items =
                [
                    new GameMenuItemDto { Label = "resume", Action = "resumeGame" },
                    new GameMenuItemDto { Label = "back to title", Action = "returnToTitle" },
                    new GameMenuItemDto { Label = "quit", Action = "quitGame" }
                ]
            }
        };
    }

    public static GameAssetDocument Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Game asset was not found.", path);
        }

        using var stream = File.OpenRead(path);
        var document = JsonSerializer.Deserialize<GameAssetDocument>(stream, JsonOptions)
            ?? throw new InvalidDataException("Game document could not be loaded.");

        document.Validate();
        return document;
    }

    public void Save(string path)
    {
        Validate();

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = File.Create(path);
        JsonSerializer.Serialize(stream, this, JsonOptions);
    }

    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(StartMap);
        ValidateMenu(TitleMenu, "title menu");
        ValidateMenu(PauseMenu, "pause menu");
    }

    public GameDefinition ToGameDefinition()
    {
        Validate();
        return new GameDefinition(Title, StartMap, ToMenuDefinition(TitleMenu), ToMenuDefinition(PauseMenu));
    }

    private static void ValidateMenu(GameMenuDto menu, string menuName)
    {
        if (menu is null || menu.Items.Count == 0)
        {
            throw new InvalidDataException($"The {menuName} must contain at least one item.");
        }

        foreach (var item in menu.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Label))
            {
                throw new InvalidDataException($"Every item in the {menuName} must have a label.");
            }

            if (!ActionMap.ContainsKey(item.Action))
            {
                throw new InvalidDataException($"Unknown menu action '{item.Action}' in the {menuName}.");
            }
        }
    }

    private static MenuDefinition ToMenuDefinition(GameMenuDto menu)
    {
        var items = menu.Items.Select(i => new MenuItemDefinition(i.Label, ActionMap[i.Action])).ToList();
        return new MenuDefinition(menu.Heading, items);
    }
}
