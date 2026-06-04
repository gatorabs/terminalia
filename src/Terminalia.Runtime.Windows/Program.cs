using Terminalia.Assets.Json;
using Terminalia.Runtime.Windows;

string mapFile;

if (args.Length == 0)
{
    var mapsDir = Path.Combine(AppContext.BaseDirectory, "maps");
    mapFile = Directory.Exists(mapsDir)
        ? Directory.GetFiles(mapsDir, "*.json").FirstOrDefault() ?? string.Empty
        : string.Empty;

    if (string.IsNullOrEmpty(mapFile))
    {
        MessageBox.Show("No map found. Expected a maps/*.json file next to the executable.", "Terminalia",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 1;
    }
}
else
{
    mapFile = Path.GetFullPath(args[0]);
    if (!File.Exists(mapFile))
    {
        MessageBox.Show($"Map file not found:\n{mapFile}", "Terminalia",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
        return 1;
    }
}

var mapName = Path.GetFileNameWithoutExtension(mapFile);
var assetRoot = Path.GetDirectoryName(Path.GetDirectoryName(mapFile)!)
    ?? throw new InvalidOperationException("Cannot determine asset root from map path.");

var loader = new JsonAssetLoader(assetRoot);
var map = loader.LoadMap(mapName);

ApplicationConfiguration.Initialize();
Application.Run(new GameWindow(map));
return 0;
