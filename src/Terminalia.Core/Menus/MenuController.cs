using Terminalia.Core.Input;

namespace Terminalia.Core.Menus;

public sealed class MenuController
{
    public MenuController(MenuDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Definition = definition;
    }

    public MenuDefinition Definition { get; }

    public int SelectedIndex { get; private set; }

    public MenuAction? Handle(InputCommand command)
    {
        var count = Definition.Items.Count;

        switch (command)
        {
            case InputCommand.MoveUp:
                SelectedIndex = (SelectedIndex - 1 + count) % count;
                return null;
            case InputCommand.MoveDown:
                SelectedIndex = (SelectedIndex + 1) % count;
                return null;
            case InputCommand.Confirm:
                return Definition.Items[SelectedIndex].Action;
            default:
                return null;
        }
    }
}
