using Terminalia.Input.Console.Abstractions;

namespace Terminalia.Input.Console.Providers;

internal sealed class ConsoleKeyboardProvider : IConsoleKeyboard
{
    public bool KeyAvailable => !System.Console.IsInputRedirected && System.Console.KeyAvailable;

    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        return System.Console.ReadKey(intercept);
    }
}
