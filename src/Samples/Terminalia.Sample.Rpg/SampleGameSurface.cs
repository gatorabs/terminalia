using Terminalia.Assets.Json;
using Terminalia.Core.Assets;
using Terminalia.Core.Ecs;
using Terminalia.Core.Ecs.Components;
using Terminalia.Core.Ecs.Systems;
using Terminalia.Core.Input;
using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;
using Terminalia.Core.ValueObjects;

namespace Terminalia.Sample.Rpg;

public sealed class SampleGameSurface : Control
{
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly CollisionSystem _collisionSystem = new();
    private readonly Queue<InputCommand> _pendingCommands = [];
    private readonly TileMap _map;
    private readonly Scene _scene;
    private readonly Entity _player;
    private DateTime _lastTick = DateTime.UtcNow;
    private TimeSpan _totalTime = TimeSpan.Zero;
    private long _frame;

    public SampleGameSurface()
    {
        BackColor = SampleTheme.AppBackground;
        DoubleBuffered = true;
        TabStop = true;

        var assetLoader = new JsonAssetLoader(Path.Combine(AppContext.BaseDirectory, "assets"));
        _map = assetLoader.LoadMap("first_dungeon");
        _scene = CreateSceneFromMap(_map);
        _player = _scene.Entities.Single(entity => entity.Name == "Player");

        _timer.Interval = 33;
        _timer.Tick += (_, _) => TickGame();
        _timer.Start();
    }

    public void HandleKey(Keys key)
    {
        var command = key switch
        {
            Keys.W or Keys.Up => InputCommand.MoveUp,
            Keys.S or Keys.Down => InputCommand.MoveDown,
            Keys.A or Keys.Left => InputCommand.MoveLeft,
            Keys.D or Keys.Right => InputCommand.MoveRight,
            Keys.Escape => InputCommand.Exit,
            _ => InputCommand.None
        };

        if (command != InputCommand.None)
        {
            _pendingCommands.Enqueue(command);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(SampleTheme.AppBackground);
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var terminal = new Rectangle(28, 28, Width - 56, Height - 56);
        if (terminal.Width <= 0 || terminal.Height <= 0)
        {
            return;
        }

        using var background = new SolidBrush(SampleTheme.TerminalBackground);
        using var border = new Pen(SampleTheme.Border);
        e.Graphics.FillRectangle(background, terminal);
        e.Graphics.DrawRectangle(border, terminal);

        DrawHeader(e.Graphics, terminal);
        DrawMap(e.Graphics, terminal);
        DrawHud(e.Graphics, terminal);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void TickGame()
    {
        var now = DateTime.UtcNow;
        var delta = now - _lastTick;
        _lastTick = now;
        _totalTime += delta;
        _frame++;

        var commands = DrainCommands();
        if (commands.Contains(InputCommand.Exit))
        {
            FindForm()?.Close();
            return;
        }

        ApplyMovementCommands(commands);
        _collisionSystem.Update(_scene, new GameTime(delta, _totalTime, (int)(_frame % int.MaxValue)));
        Invalidate();
    }

    private IReadOnlyCollection<InputCommand> DrainCommands()
    {
        if (_pendingCommands.Count == 0)
        {
            return [];
        }

        var commands = _pendingCommands.ToArray();
        _pendingCommands.Clear();
        return commands;
    }

    private void ApplyMovementCommands(IReadOnlyCollection<InputCommand> commands)
    {
        var velocity = commands.LastOrDefault(command => command is
            InputCommand.MoveUp or
            InputCommand.MoveDown or
            InputCommand.MoveLeft or
            InputCommand.MoveRight) switch
        {
            InputCommand.MoveUp => new Velocity(0, -1),
            InputCommand.MoveDown => new Velocity(0, 1),
            InputCommand.MoveLeft => new Velocity(-1, 0),
            InputCommand.MoveRight => new Velocity(1, 0),
            _ => Velocity.Zero
        };

        _player.GetComponent<VelocityComponent>()!.Velocity = velocity;
    }

    private void DrawHeader(Graphics graphics, Rectangle terminal)
    {
        var header = new Rectangle(terminal.X, terminal.Y, terminal.Width, 46);
        using var headerBrush = new SolidBrush(SampleTheme.TerminalHeader);
        graphics.FillRectangle(headerBrush, header);

        TextRenderer.DrawText(
            graphics,
            "Terminalia Sample RPG",
            SampleTheme.TitleFont,
            new Rectangle(header.X + 18, header.Y, header.Width - 36, header.Height),
            SampleTheme.PrimaryText,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
    }

    private void DrawMap(Graphics graphics, Rectangle terminal)
    {
        var origin = new Point(terminal.X + 30, terminal.Y + 76);
        var lineHeight = TextRenderer.MeasureText("M", SampleTheme.TerminalFont).Height + 4;

        for (var y = 0; y < _map.Height; y++)
        {
            var xOffset = origin.X;
            for (var x = 0; x < _map.Width; x++)
            {
                var tile = GetVisibleTile(x, y);
                var textSize = TextRenderer.MeasureText(tile.ToString(), SampleTheme.TerminalFont);
                TextRenderer.DrawText(
                    graphics,
                    tile.ToString(),
                    SampleTheme.TerminalFont,
                    new Point(xOffset, origin.Y + (y * lineHeight)),
                    GetTileColor(tile));
                xOffset += textSize.Width - 3;
            }
        }
    }

    private void DrawHud(Graphics graphics, Rectangle terminal)
    {
        var hud = new Rectangle(terminal.Right - 260, terminal.Bottom - 112, 230, 84);
        using var hudBrush = new SolidBrush(Color.FromArgb(24, 33, 42));
        using var border = new Pen(Color.FromArgb(53, 65, 80));
        graphics.FillRectangle(hudBrush, hud);
        graphics.DrawRectangle(border, hud);

        var lines = new[]
        {
            $"Map: {_map.Name}",
            $"Size: {_map.Width}x{_map.Height}",
            "Move: WASD or arrows",
            "Exit: ESC"
        };

        for (var i = 0; i < lines.Length; i++)
        {
            TextRenderer.DrawText(
                graphics,
                lines[i],
                SampleTheme.HudFont,
                new Rectangle(hud.X + 12, hud.Y + 10 + (i * 18), hud.Width - 24, 18),
                i == 0 ? Color.FromArgb(147, 197, 253) : Color.FromArgb(203, 213, 225),
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }
    }

    private char GetVisibleTile(int x, int y)
    {
        var playerTransform = _player.GetComponent<TransformComponent>()!;
        if (playerTransform.Position.X == x && playerTransform.Position.Y == y)
        {
            return '@';
        }

        var entity = _scene.Entities
            .Where(item => item.Name != "Player")
            .Select(item => new
            {
                Transform = item.GetComponent<TransformComponent>(),
                Render = item.GetComponent<RenderComponent>()
            })
            .Where(item => item.Transform?.Position.X == x && item.Transform.Position.Y == y && item.Render is not null)
            .OrderBy(item => item.Render!.Layer)
            .LastOrDefault();

        return entity?.Render?.Character ?? (_map.Tiles[y][x] == '#' ? '#' : '.');
    }

    private static Scene CreateSceneFromMap(TileMap map)
    {
        var scene = new Scene(map.Name);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.Tiles[y][x];
                var position = new Position(x, y);

                switch (tile)
                {
                    case '#':
                        scene.AddEntity(CreateEntity("Stone Wall", position, '#', layer: 1, Velocity.Zero, solid: true));
                        break;
                    case '@':
                        scene.AddEntity(CreateEntity("Player", position, '@', layer: 2, Velocity.Zero, solid: true));
                        break;
                    case '*':
                        scene.AddEntity(CreateEntity("Relic", position, '*', layer: 1, Velocity.Zero, solid: false));
                        break;
                }
            }
        }

        return scene;
    }

    private static Entity CreateEntity(
        string name,
        Position position,
        char character,
        int layer,
        Velocity velocity,
        bool solid)
    {
        var entity = new Entity(name);
        entity.AddComponent(new TransformComponent(position));
        entity.AddComponent(new RenderComponent(character, layer));
        entity.AddComponent(new VelocityComponent(velocity));
        entity.AddComponent(new ColliderComponent(new Terminalia.Core.ValueObjects.Size(1, 1), solid));
        return entity;
    }

    private static Color GetTileColor(char tile)
    {
        return tile switch
        {
            '#' => Color.FromArgb(148, 163, 184),
            '.' => Color.FromArgb(71, 85, 105),
            '@' => Color.FromArgb(96, 165, 250),
            '*' => Color.FromArgb(250, 204, 21),
            _ => SampleTheme.PrimaryText
        };
    }
}
