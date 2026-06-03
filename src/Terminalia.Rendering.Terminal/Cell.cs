namespace Terminalia.Rendering.Terminal;

public readonly record struct Cell(
    char Character,
    ConsoleColor Foreground,
    ConsoleColor Background)
{
    public static Cell Empty { get; } = new(' ', ConsoleColor.Gray, ConsoleColor.Black);
}
