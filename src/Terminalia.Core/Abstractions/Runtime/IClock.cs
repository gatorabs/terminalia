using Terminalia.Core.Runtime;

namespace Terminalia.Core.Abstractions.Runtime;

public interface IClock
{
    GameTime GetGameTime();
    void WaitForNextFrame(int targetFps);
}
