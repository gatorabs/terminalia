namespace Terminalia.Core.Assets;

public interface IAssetLoader
{
    TileMap LoadMap(string assetName);

    Sprite LoadSprite(string assetName);

    Animation LoadAnimation(string assetName);
}
