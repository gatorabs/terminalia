using Terminalia.Assets.Json;
using Terminalia.Editor.Selection.Descriptors;

namespace Terminalia.Editor.Selection;

public sealed class SelectableRegistry
{
    private readonly List<ITileDescriptor> _descriptors = [];
    private readonly ITileDescriptor _fallback = new DefaultTileDescriptor();

    public SelectableRegistry()
    {
        Register(new PlayerTileDescriptor());
        Register(new RelicTileDescriptor());
    }

    public void Register(ITileDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        _descriptors.Add(descriptor);
    }

    public ITileDescriptor Resolve(char tile)
    {
        return _descriptors.FirstOrDefault(d => d.CanDescribe(tile)) ?? _fallback;
    }

    public SelectedObjectInfo Describe(MapAssetDocument document, Point cell)
    {
        return Resolve(document.GetTile(cell.X, cell.Y)).Describe(document, cell);
    }

    public void ApplyProperty(MapAssetDocument document, Point cell, string key, string value)
    {
        Resolve(document.GetTile(cell.X, cell.Y)).ApplyProperty(document, cell, key, value);
    }
}
