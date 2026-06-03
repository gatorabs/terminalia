namespace Terminalia.Core.Ecs.Components;

public sealed class AnimatedSpriteComponent : IComponent
{
    private readonly List<char> _frames;

    public AnimatedSpriteComponent(IEnumerable<char> frames, TimeSpan frameDuration)
    {
        ArgumentNullException.ThrowIfNull(frames);

        if (frameDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(frameDuration), "Frame duration must be greater than zero.");
        }

        _frames = frames.ToList();
        if (_frames.Count == 0)
        {
            throw new ArgumentException("Animated sprite must contain at least one frame.", nameof(frames));
        }

        FrameDuration = frameDuration;
    }

    public IReadOnlyList<char> Frames => _frames;

    public TimeSpan FrameDuration { get; }

    public TimeSpan Elapsed { get; set; }

    public int CurrentFrameIndex { get; private set; }

    public char CurrentFrame => _frames[CurrentFrameIndex];

    public void AdvanceFrame()
    {
        CurrentFrameIndex = (CurrentFrameIndex + 1) % _frames.Count;
    }
}
