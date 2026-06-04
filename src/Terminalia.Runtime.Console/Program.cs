using Terminalia.Assets.Json;
using Terminalia.Core.Runtime;
using Terminalia.Infrastructure;
using Terminalia.Input.Console.Providers;
using Terminalia.Rendering.Terminal;
using Terminalia.Runtime;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: terminalia-console <path-to-map.json>");
    return 1;
}

var mapFile = Path.GetFullPath(args[0]);
if (!File.Exists(mapFile))
{
    Console.Error.WriteLine($"Map file not found: {mapFile}");
    return 1;
}

var mapName = Path.GetFileNameWithoutExtension(mapFile);
var assetRoot = Path.GetDirectoryName(Path.GetDirectoryName(mapFile)!)
    ?? throw new InvalidOperationException("Cannot determine asset root from map path.");

var loader = new JsonAssetLoader(assetRoot);
var map = loader.LoadMap(mapName);
var renderer = new TerminalRenderer(map.Width, map.Height);
var inputProvider = new ConsoleInputProvider();
var scene = SceneAssembler.Assemble(map, renderer);
var game = new TileMapGame(scene, renderer, inputProvider);
var clock = new SystemClock();
var loop = new GameLoop(game, clock, new FrameRateSettings(30));

Console.CursorVisible = false;
Console.Clear();
loop.Run();
Console.CursorVisible = true;

return 0;
