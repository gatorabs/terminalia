using Terminalia.Assets.Json;

namespace Terminalia.Editor.Selection;

public interface ITileDescriptor
{
    bool CanDescribe(char tile);

    SelectedObjectInfo Describe(MapAssetDocument document, Point cell);

    void ApplyProperty(MapAssetDocument document, Point cell, string key, string value);
}
