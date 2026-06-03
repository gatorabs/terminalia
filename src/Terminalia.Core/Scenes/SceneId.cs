namespace Terminalia.Core.Scenes;

public readonly record struct SceneId(Guid Value)
{
    public static SceneId New()
    {
        return new SceneId(Guid.NewGuid());
    }
}
