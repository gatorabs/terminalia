using Terminalia.Assets.Json;
using Terminalia.Runtime.Windows;

if (args.Length == 0)
{
    MessageBox.Show("Usage: game.exe <path-to-map.json>", "Terminalia",
        MessageBoxButtons.OK, MessageBoxIcon.Information);
    return 1;
}

var mapFile = Path.GetFullPath(args[0]);
if (!File.Exists(mapFile))
{
    MessageBox.Show($"Map file not found:\n{mapFile}", "Terminalia",
        MessageBoxButtons.OK, MessageBoxIcon.Error);
    return 1;
}

var mapName = Path.GetFileNameWithoutExtension(mapFile);
var assetRoot = Path.GetDirectoryName(Path.GetDirectoryName(mapFile)!)
    ?? throw new InvalidOperationException("Cannot determine asset root from map path.");

var loader = new JsonAssetLoader(assetRoot);
var map = loader.LoadMap(mapName);

ApplicationConfiguration.Initialize();
Application.Run(new GameWindow(map));
return 0;
