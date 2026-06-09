namespace Terminalia.Core.Assets;

public static class EntityPropertyKeys
{
    public const string Speed = "speed";
    public const string Health = "health";
    public const string Collectible = "collectible";
    public const string DisappearOnCollect = "disappearOnCollect";

    public const int DefaultSpeed = 1;
    public const int MinSpeed = 1;
    public const int MaxSpeed = 5;
    public const int DefaultHealth = 3;
    public const int MinHealth = 1;
    public const int MaxHealth = 999;
    public const bool DefaultCollectible = true;
    public const bool DefaultDisappearOnCollect = true;
}
