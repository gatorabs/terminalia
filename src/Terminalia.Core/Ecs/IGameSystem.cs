using Terminalia.Core.Runtime;
using Terminalia.Core.Scenes;

namespace Terminalia.Core.Ecs;

public interface IGameSystem
{
    void Update(Scene scene, GameTime time);
}
