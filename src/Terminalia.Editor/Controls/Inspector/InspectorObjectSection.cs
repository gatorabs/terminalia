using Terminalia.Editor.Selection;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Controls;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor.Controls.Inspector;

public sealed class InspectorObjectSection : Panel
{
    private const int TitleRowHeight = 32;
    private const int PropertyRowHeight = 30;
    private const int SectionChromeHeight = 78;

    private readonly Label _titleLabel = new();
    private readonly TableLayoutPanel _propertiesLayout = new();
    private readonly CheckBox _kernelPreviewCheckBox = new();
    private bool _suppressEvents;

    public event EventHandler<ObjectPropertyEditedEventArgs>? PropertyEdited;
    public event EventHandler<bool>? KernelPreviewToggled;

    public InspectorObjectSection()
    {
        Dock = DockStyle.Fill;
        BackColor = EditorColors.Section;

        _titleLabel.Dock = DockStyle.Top;
        _titleLabel.Height = TitleRowHeight;
        _titleLabel.AutoEllipsis = true;
        _titleLabel.Font = EditorFonts.UiFont;
        _titleLabel.ForeColor = EditorColors.PrimaryText;
        _titleLabel.TextAlign = ContentAlignment.MiddleLeft;

        _propertiesLayout.Dock = DockStyle.Fill;
        _propertiesLayout.ColumnCount = 2;
        _propertiesLayout.BackColor = EditorColors.Section;
        _propertiesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        _propertiesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _kernelPreviewCheckBox.Text = "Mostrar kernel da câmera";
        _kernelPreviewCheckBox.Dock = DockStyle.Bottom;
        _kernelPreviewCheckBox.Height = PropertyRowHeight;
        _kernelPreviewCheckBox.Font = EditorFonts.UiFont;
        _kernelPreviewCheckBox.BackColor = EditorColors.Section;
        _kernelPreviewCheckBox.ForeColor = EditorColors.PrimaryText;
        _kernelPreviewCheckBox.Visible = false;
        _kernelPreviewCheckBox.CheckedChanged += (_, _) =>
        {
            if (!_suppressEvents)
            {
                KernelPreviewToggled?.Invoke(this, _kernelPreviewCheckBox.Checked);
            }
        };

        Controls.Add(_propertiesLayout);
        Controls.Add(_kernelPreviewCheckBox);
        Controls.Add(_titleLabel);
        ClearObject();
    }

    public int PreferredSectionHeight { get; private set; } = 128;

    public void ShowObject(SelectedObjectInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        _titleLabel.Text = string.IsNullOrWhiteSpace(info.Description)
            ? $"'{info.Tile}' — {info.DisplayName}"
            : $"'{info.Tile}' — {info.DisplayName} ({info.Description})";

        RebuildPropertyRows(info.Properties);

        _suppressEvents = true;
        _kernelPreviewCheckBox.Visible = info.ShowKernelPreview;
        if (!info.ShowKernelPreview)
        {
            _kernelPreviewCheckBox.Checked = false;
        }
        _suppressEvents = false;

        var kernelRow = info.ShowKernelPreview ? PropertyRowHeight : 0;
        PreferredSectionHeight = SectionChromeHeight + TitleRowHeight + (info.Properties.Count * PropertyRowHeight) + kernelRow;
    }

    public void ClearObject()
    {
        _titleLabel.Text = "Selecione um objeto no mapa.";
        RebuildPropertyRows([]);

        _suppressEvents = true;
        _kernelPreviewCheckBox.Visible = false;
        _kernelPreviewCheckBox.Checked = false;
        _suppressEvents = false;

        PreferredSectionHeight = 128;
    }

    private void RebuildPropertyRows(IReadOnlyList<EditableProperty> properties)
    {
        _propertiesLayout.SuspendLayout();
        _propertiesLayout.Controls.Clear();
        _propertiesLayout.RowStyles.Clear();
        _propertiesLayout.RowCount = properties.Count;

        for (var i = 0; i < properties.Count; i++)
        {
            _propertiesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, PropertyRowHeight));
            _propertiesLayout.Controls.Add(EditorControls.CreateFieldLabel(properties[i].Label), 0, i);
            _propertiesLayout.Controls.Add(InspectorPropertyEditors.Create(properties[i], RaisePropertyEdited), 1, i);
        }

        _propertiesLayout.ResumeLayout();
    }

    private void RaisePropertyEdited(string key, string value)
    {
        if (!_suppressEvents)
        {
            PropertyEdited?.Invoke(this, new ObjectPropertyEditedEventArgs(key, value));
        }
    }
}

public sealed class ObjectPropertyEditedEventArgs(string key, string value) : EventArgs
{
    public string Key { get; } = key;
    public string Value { get; } = value;
}
