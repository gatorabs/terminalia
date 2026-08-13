using Terminalia.Core.Assets;
using Terminalia.Runtime.Windows.GameHost;

namespace Terminalia.Runtime.Windows;

public sealed partial class GameWindow : Form
{
    private const int GamePadding = GameViewportDefaults.GamePadding;
    private const int HintBarHeight = GameViewportDefaults.HintBarHeight;

    private readonly WinFormsGameHost _host;
    private readonly TileMap _map;
    private readonly GameDefinition? _definition;
    private readonly Size _kernelSize;
    private readonly Font _hintFont = new(FontFamily.GenericMonospace, 8, FontStyle.Regular);

    private readonly string? _assetDirectory;

    public GameWindow(GameDefinition definition, TileMap startMap, string? assetDirectory = null)
        : this(startMap, assetDirectory)
    {
        ArgumentNullException.ThrowIfNull(definition);
        _definition = definition;
        Text = definition.Title;
    }

    public GameWindow(TileMap map, string? assetDirectory = null)
    {
        _map = map;
        _assetDirectory = assetDirectory;
        _kernelSize = GameViewportDefaults.ComputeKernelSize(map.CameraZoom);

        var defaultCellSize = GameViewportDefaults.ComputeCellSize(
            GameViewportDefaults.ComputeAvailableArea(GameViewportDefaults.WindowClientSize), _kernelSize);
        _host = new WinFormsGameHost { CellSize = defaultCellSize };

        Text = map.Name;
        BackColor = Color.Black;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;
        MinimumSize = ComputeMinimumSize();
        ClientSize = GameViewportDefaults.WindowClientSize;

        _host.GameStopped += (_, _) =>
        {
            if (!IsDisposed)
            {
                BeginInvoke(Close);
            }
        };

        _host.TransitionFailed += (_, msg) =>
        {
            if (!IsDisposed)
            {
                BeginInvoke(() => MessageBox.Show(this, msg, "Zone Transition", MessageBoxButtons.OK, MessageBoxIcon.Warning));
            }
        };

        TryUseDarkTitleBar();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _host.Dispose();
            _hintFont.Dispose();
        }
        base.Dispose(disposing);
    }
}
