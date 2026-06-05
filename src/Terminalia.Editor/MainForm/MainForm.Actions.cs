using System.Diagnostics;
using Terminalia.Assets.Json;
using Terminalia.Editor.Controls.Inspector;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor;

public sealed partial class MainForm
{
    private void LoadDocument(MapAssetDocument document, string? currentFile, bool markClean)
    {
        if (_shell.Runtime.IsRunning)
        {
            _shell.Runtime.StopGame();
            _shell.ExitGameMode();
            _shell.Inspector.SetRunState(false);
        }

        _state.Document = document;
        _state.CurrentFile = currentFile;
        _state.SelectedZone = null;

        _shell.Canvas.LoadDocument(document);
        _shell.Canvas.SelectedZone = null;
        _shell.Runtime.LoadDocument(document.ToTileMap());
        _shell.Inspector.LoadDocument(document.Name, document.Width, document.Height);
        _shell.Inspector.LoadZones(document.GetZoneNames());

        if (markClean)
        {
            MarkClean();
        }
        else
        {
            MarkDirty();
        }

        UpdateStatus();
    }

    private void NewMap()
    {
        var name = PromptMapName();
        if (name is null)
        {
            return;
        }
        var doc = _fileService.CreateDefaultMap();
        if (!string.IsNullOrWhiteSpace(name))
        {
            doc.Name = name;
        }
        AddTab(doc, filePath: null, markClean: true);
    }

    private string? PromptMapName()
    {
        using var form = new Form
        {
            Text = "New Map",
            Size = new Size(300, 140),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = EditorColors.AppBackground,
            ForeColor = EditorColors.PrimaryText,
            Font = EditorFonts.UiFont
        };

        var label = new Label
        {
            Text = "Map name:",
            Location = new Point(12, 12),
            AutoSize = true,
            ForeColor = EditorColors.SecondaryText
        };

        var textBox = new TextBox
        {
            Location = new Point(12, 34),
            Width = 262,
            Text = "new_map",
            BackColor = EditorColors.Surface,
            ForeColor = EditorColors.PrimaryText,
            BorderStyle = BorderStyle.FixedSingle
        };

        var ok = new Button
        {
            Text = "Create",
            DialogResult = DialogResult.OK,
            Location = new Point(12, 68),
            Width = 126,
            BackColor = EditorColors.Accent,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Black
        };
        ok.FlatAppearance.BorderSize = 0;

        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(148, 68),
            Width = 126,
            BackColor = EditorColors.Surface,
            FlatStyle = FlatStyle.Flat,
            ForeColor = EditorColors.PrimaryText
        };
        cancel.FlatAppearance.BorderColor = EditorColors.BorderSoft;

        form.Controls.AddRange([label, textBox, ok, cancel]);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        textBox.SelectAll();

        return form.ShowDialog(this) == DialogResult.OK ? textBox.Text.Trim() : null;
    }

    private void OpenMap()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Terminalia map (*.json)|*.json|All files (*.*)|*.*",
            Title = "Open Terminalia map"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var existingIndex = _tabs.FindIndex(t =>
            string.Equals(t.State.CurrentFile, dialog.FileName, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            SwitchToTab(existingIndex);
            return;
        }

        AddTab(_fileService.LoadMap(dialog.FileName), dialog.FileName, markClean: true);
    }

    private bool SaveMap()
    {
        if (_state.CurrentFile is null)
        {
            return SaveMapAs();
        }

        _fileService.SaveMap(_state.Document, _state.CurrentFile);
        MarkClean();
        UpdateStatus();
        return true;
    }

    private bool SaveMapAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Terminalia map (*.json)|*.json|All files (*.*)|*.*",
            Title = "Save Terminalia map",
            FileName = $"{_state.Document.Name}.json"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return false;
        }

        _state.CurrentFile = dialog.FileName;
        _fileService.SaveMap(_state.Document, _state.CurrentFile);
        MarkClean();
        UpdateStatus();
        return true;
    }

    private async void ExportGame()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose export destination",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var outputDir = Path.Combine(dialog.SelectedPath, _state.Document.Name);
        var progress = new Progress<string>(msg => BeginInvoke(() => _loadingOverlay.ShowLoading(msg)));

        _loadingOverlay.ShowLoading("Exporting game...");

        var result = await _exportService.ExportAsync(_state.Document, outputDir, progress);

        _loadingOverlay.HideLoading();

        if (result.IsSuccess)
        {
            var response = MessageBox.Show(this,
                $"Exported to:\n{result.OutputDirectory}\n\nOpen folder?",
                "Export Complete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (response == DialogResult.Yes)
            {
                Process.Start("explorer.exe", result.OutputDirectory);
            }
        }
        else
        {
            MessageBox.Show(this, result.ErrorMessage, "Export Failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LaunchGameWindow()
    {
        var assetDirectory = _state.CurrentFile is not null
            ? Path.GetDirectoryName(_state.CurrentFile)
            : null;
        var window = new Terminalia.Runtime.Windows.GameWindow(_state.Document.ToTileMap(), cellSize: _shell.Canvas.CellSize, assetDirectory: assetDirectory);
        window.Show(this);
    }

    private void ShowEditor()
    {
        if (_shell.Visible)
        {
            return;
        }

        _shell.Visible = true;
        _startPage.Visible = false;
        _shell.BringToFront();
        _loadingOverlay.BringToFront();
    }

    private void ApplyMapProperties(MapPropertiesChangedEventArgs args)
    {
        _state.Document.Name = string.IsNullOrWhiteSpace(args.Name)
            ? "untitled_map"
            : args.Name.Trim();
        _state.Document.Resize(args.Width, args.Height);
        _shell.Canvas.RefreshDocument();
        _shell.Runtime.LoadDocument(_state.Document.ToTileMap());
        UpdateStatus();
    }

    private void ToggleEmbeddedRun()
    {
        if (_shell.Runtime.IsRunning)
        {
            _shell.Runtime.StopGame();
            _shell.ExitGameMode();
        }
        else
        {
            var assetDirectory = _state.CurrentFile is not null
                ? Path.GetDirectoryName(_state.CurrentFile)
                : null;
            _shell.EnterGameMode();
            _shell.Runtime.StartGame(assetDirectory);
            _shell.Runtime.Focus();
        }

        _shell.Inspector.SetRunState(_shell.Runtime.IsRunning);
        UpdateStatus();
    }

    private void MarkDirty()
    {
        _state.HasUnsavedChanges = true;
        _shell.Runtime.LoadDocument(_state.Document.ToTileMap(), preserveRunState: true);
        RefreshTabBar();
        UpdateStatus();
    }

    private void MarkClean()
    {
        _state.HasUnsavedChanges = false;
        RefreshTabBar();
        UpdateStatus();
    }

    private void RefreshAvailableMaps()
    {
        var fromDisk = Enumerable.Empty<string>();
        if (_state.CurrentFile is not null)
        {
            var dir = Path.GetDirectoryName(_state.CurrentFile);
            if (dir is not null)
            {
                fromDisk = Directory.GetFiles(dir, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .OfType<string>();
            }
        }

        var fromTabs = _tabs
            .Select(t => t.State.CurrentFile is not null
                ? Path.GetFileNameWithoutExtension(t.State.CurrentFile)
                : t.State.Document.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .OfType<string>();

        var all = fromDisk
            .Concat(fromTabs)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n);

        _shell.Inspector.LoadAvailableMaps(all);
    }

    private void UpdateStatus()
    {
        var dirtyMarker = _state.HasUnsavedChanges ? "*" : "";
        var cellText = _state.CursorCell is { } cell
            ? $"Cell: {cell.X},{cell.Y}"
            : "Cell: -";

        Text = $"Terminalia Editor{dirtyMarker}";
        _shell.SetStatus(
            $"{_state.Document.Name}{dirtyMarker} | {_state.Document.Width}x{_state.Document.Height} | {cellText} | Tool: {_state.ToolMode} | Tile: {_state.SelectedTile} | {_state.CurrentFile ?? "Unsaved"}");
    }

}
