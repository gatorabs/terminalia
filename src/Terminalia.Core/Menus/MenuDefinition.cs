namespace Terminalia.Core.Menus;

public sealed class MenuDefinition
{
    public MenuDefinition(string heading, IReadOnlyList<MenuItemDefinition> items)
    {
        ArgumentNullException.ThrowIfNull(heading);
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("A menu must contain at least one item.", nameof(items));
        }

        Heading = heading;
        Items = items;
    }

    public string Heading { get; }

    public IReadOnlyList<MenuItemDefinition> Items { get; }
}
