namespace Terminalia.Input.Console;

internal interface IConsoleKeyboard
{
    bool KeyAvailable { get; }

    ConsoleKeyInfo ReadKey(bool intercept);
}
