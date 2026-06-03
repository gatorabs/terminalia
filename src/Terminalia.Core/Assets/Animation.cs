namespace Terminalia.Core.Assets;

public sealed class Animation
{
    private readonly List<Sprite> _frames;

    public Animation(string name, TimeSpan frameDuration, IEnumerable<Sprite> frames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (frameDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(frameDuration), "Frame duration must be greater than zero.");
        }
        ArgumentNullException.ThrowIfNull(frames);

        Name = name;
        FrameDuration = frameDuration;
        _frames = frames.ToList();

        if (_frames.Count == 0)
        {
            throw new ArgumentException("Animation must contain at least one frame.", nameof(frames));
        }
    }

    public string Name { get; }

    public TimeSpan FrameDuration { get; }

    public IReadOnlyList<Sprite> Frames => _frames;
}
