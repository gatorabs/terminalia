namespace Terminalia.Core.Assets;

public sealed class Sprite
{
    private readonly List<string> _lines;

    public Sprite(string name, IEnumerable<string> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(lines);

        Name = name;
        _lines = lines.ToList();
    }

    public string Name { get; }

    public IReadOnlyList<string> Lines => _lines;
}
