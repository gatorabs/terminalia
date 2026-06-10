using Terminalia.Assets.Json;
using Terminalia.Core.Menus;

namespace Terminalia.UnitTests.Assets;

public sealed class GameAssetDocumentTests : IDisposable
{
    private readonly string _tempDirectory;

    public GameAssetDocumentTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"terminalia-game-doc-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void GameAssetDocument_Should_Round_Trip_Through_Save_And_Load()
    {
        var path = Path.Combine(_tempDirectory, "game.json");
        var document = GameAssetDocument.CreateDefault("dungeon_1");
        document.Title = "My Adventure";
        document.TitleMenu.Heading = "ADVENTURE";

        document.Save(path);
        var loaded = GameAssetDocument.Load(path);

        Assert.Equal("My Adventure", loaded.Title);
        Assert.Equal("dungeon_1", loaded.StartMap);
        Assert.Equal("ADVENTURE", loaded.TitleMenu.Heading);
        Assert.Equal(document.PauseMenu.Items.Count, loaded.PauseMenu.Items.Count);
        Assert.Equal("returnToTitle", loaded.PauseMenu.Items[1].Action);
    }

    [Fact]
    public void GameAssetDocument_Should_Serialize_In_CamelCase()
    {
        var path = Path.Combine(_tempDirectory, "game.json");
        GameAssetDocument.CreateDefault("dungeon_1").Save(path);

        var json = File.ReadAllText(path);

        Assert.Contains("\"startMap\"", json);
        Assert.Contains("\"titleMenu\"", json);
        Assert.Contains("\"startGame\"", json);
    }

    [Fact]
    public void Validate_Should_Reject_Empty_Title()
    {
        var document = GameAssetDocument.CreateDefault("dungeon_1");
        document.Title = " ";

        Assert.Throws<ArgumentException>(document.Validate);
    }

    [Fact]
    public void Validate_Should_Reject_Empty_Start_Map()
    {
        var document = GameAssetDocument.CreateDefault("dungeon_1");
        document.StartMap = "";

        Assert.Throws<ArgumentException>(document.Validate);
    }

    [Fact]
    public void Validate_Should_Reject_Menu_Without_Items()
    {
        var document = GameAssetDocument.CreateDefault("dungeon_1");
        document.PauseMenu.Items.Clear();

        Assert.Throws<InvalidDataException>(document.Validate);
    }

    [Fact]
    public void Validate_Should_Reject_Unknown_Action()
    {
        var document = GameAssetDocument.CreateDefault("dungeon_1");
        document.TitleMenu.Items[0].Action = "explode";

        Assert.Throws<InvalidDataException>(document.Validate);
    }

    [Fact]
    public void CreateDefault_Should_Be_Valid_With_Expected_Actions()
    {
        var document = GameAssetDocument.CreateDefault("dungeon_1");

        document.Validate();

        Assert.Equal(["startGame", "quitGame"], document.TitleMenu.Items.Select(i => i.Action));
        Assert.Equal(["resumeGame", "returnToTitle", "quitGame"], document.PauseMenu.Items.Select(i => i.Action));
    }

    [Fact]
    public void ToGameDefinition_Should_Map_Actions_To_Enum()
    {
        var definition = GameAssetDocument.CreateDefault("dungeon_1").ToGameDefinition();

        Assert.Equal(MenuAction.StartGame, definition.TitleMenu.Items[0].Action);
        Assert.Equal(MenuAction.QuitGame, definition.TitleMenu.Items[1].Action);
        Assert.Equal(MenuAction.ResumeGame, definition.PauseMenu.Items[0].Action);
        Assert.Equal(MenuAction.ReturnToTitle, definition.PauseMenu.Items[1].Action);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }
}
