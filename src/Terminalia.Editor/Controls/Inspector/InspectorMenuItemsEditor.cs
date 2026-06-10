using Terminalia.Assets.Json;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;

namespace Terminalia.Editor.Controls.Inspector;

public sealed class InspectorMenuItemsEditor : Panel
{
    private readonly TextBox _headingTextBox = new();
    private readonly TextBox _labelTextBox = new();
    private readonly ComboBox _actionCombo = new();
    private readonly ListBox _itemsList = new();
    private List<GameMenuItemDto> _items = [];

    public InspectorMenuItemsEditor()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.Section;

        EditorControls.StyleTextBox(_headingTextBox);
        EditorControls.StyleTextBox(_labelTextBox);
        EditorControls.StyleComboBox(_actionCombo);
        _actionCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        foreach (var action in GameAssetDocument.KnownActions)
        {
            _actionCombo.Items.Add(action);
        }
        _actionCombo.SelectedIndex = 0;

        EditorControls.StyleListBox(_itemsList);
        _itemsList.SelectedIndexChanged += (_, _) =>
        {
            if (_itemsList.SelectedIndex is var index && index >= 0 && index < _items.Count)
            {
                _labelTextBox.Text = _items[index].Label;
                _actionCombo.SelectedItem = _items[index].Action;
            }
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            BackColor = EditorColors.Section,
            Padding = new Padding(0, 2, 0, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        var editButtons = EditorControls.CreateButtonPair("Add / Update", AddOrUpdateItem, "Remove", RemoveItem);
        var moveButtons = EditorControls.CreateButtonPair("Move up", () => MoveItem(-1), "Move down", () => MoveItem(1));

        layout.Controls.Add(EditorControls.CreateFieldLabel("Heading"), 0, 0);
        layout.Controls.Add(_headingTextBox, 1, 0);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Label"), 0, 1);
        layout.Controls.Add(_labelTextBox, 1, 1);
        layout.Controls.Add(EditorControls.CreateFieldLabel("Action"), 0, 2);
        layout.Controls.Add(_actionCombo, 1, 2);
        layout.Controls.Add(editButtons, 0, 3);
        layout.SetColumnSpan(editButtons, 2);
        layout.Controls.Add(_itemsList, 0, 4);
        layout.SetColumnSpan(_itemsList, 2);
        layout.Controls.Add(moveButtons, 0, 5);
        layout.SetColumnSpan(moveButtons, 2);

        Controls.Add(layout);
    }

    public void LoadMenu(GameMenuDto menu)
    {
        ArgumentNullException.ThrowIfNull(menu);
        _headingTextBox.Text = menu.Heading;
        _items = menu.Items.Select(CloneItem).ToList();
        RefreshList(selectedIndex: _items.Count > 0 ? 0 : -1);
    }

    public GameMenuDto ToMenu()
    {
        return new GameMenuDto
        {
            Heading = _headingTextBox.Text.Trim(),
            Items = _items.Select(CloneItem).ToList()
        };
    }

    private static GameMenuItemDto CloneItem(GameMenuItemDto item)
    {
        return new GameMenuItemDto { Label = item.Label, Action = item.Action };
    }

    private void AddOrUpdateItem()
    {
        var label = _labelTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(label) || _actionCombo.SelectedItem is not string action)
        {
            return;
        }

        var item = new GameMenuItemDto { Label = label, Action = action };
        var index = _itemsList.SelectedIndex;
        if (index >= 0 && index < _items.Count)
        {
            _items[index] = item;
            RefreshList(index);
        }
        else
        {
            _items.Add(item);
            RefreshList(_items.Count - 1);
        }
    }

    private void RemoveItem()
    {
        var index = _itemsList.SelectedIndex;
        if (index < 0 || index >= _items.Count)
        {
            return;
        }

        _items.RemoveAt(index);
        RefreshList(Math.Min(index, _items.Count - 1));
    }

    private void MoveItem(int offset)
    {
        var index = _itemsList.SelectedIndex;
        var target = index + offset;
        if (index < 0 || index >= _items.Count || target < 0 || target >= _items.Count)
        {
            return;
        }

        (_items[index], _items[target]) = (_items[target], _items[index]);
        RefreshList(target);
    }

    private void RefreshList(int selectedIndex)
    {
        _itemsList.Items.Clear();
        foreach (var item in _items)
        {
            _itemsList.Items.Add($"{item.Label}  →  {item.Action}");
        }

        if (selectedIndex >= 0 && selectedIndex < _itemsList.Items.Count)
        {
            _itemsList.SelectedIndex = selectedIndex;
        }
    }
}
