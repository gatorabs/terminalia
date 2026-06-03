namespace Terminalia.Input.Console.Abstractions;

internal interface IConsoleKeyboard
{
    bool KeyAvailable { get; }

    ConsoleKeyInfo ReadKey(bool intercept);
}
