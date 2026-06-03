namespace Terminalia.Input.Console;

internal sealed class SystemConsoleKeyboard : IConsoleKeyboard
{
    public bool KeyAvailable => !System.Console.IsInputRedirected && System.Console.KeyAvailable;

    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        return System.Console.ReadKey(intercept);
    }
}
