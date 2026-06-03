namespace Terminalia.Core.Runtime;

public interface IGame
{
    bool IsRunning { get; }

    void Initialize();

    void HandleInput();

    void Update(GameTime time);

    void Render();

    void Stop();
}
