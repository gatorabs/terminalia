namespace Terminalia.Core.Runtime;

public interface IClock
{
    GameTime GetGameTime();

    void WaitForNextFrame(int targetFps);
}
