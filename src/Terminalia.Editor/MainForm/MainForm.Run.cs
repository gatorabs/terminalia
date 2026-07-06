using System.Diagnostics;
using Terminalia.Assets.Json;

namespace Terminalia.Editor;

public sealed partial class MainForm
{
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

        SaveAllMaps();
        var outputDir = Path.Combine(dialog.SelectedPath, _state.Document.Name);
        var progress = new Progress<string>(msg => BeginInvoke(() => _loadingOverlay.ShowLoading(msg)));

        _loadingOverlay.ShowLoading("Exporting game...");

        var result = await _exportService.ExportAsync(_state.Document, _gameDocument, outputDir, progress);

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
        SaveAllMaps();
        var window = new Terminalia.Runtime.Windows.GameWindow(
            BuildRunDefinition(), _state.Document.ToTileMap(), assetDirectory: GetAssetDirectory());
        window.Show(this);
    }

    private Terminalia.Core.Assets.GameDefinition BuildRunDefinition()
    {
        var document = new GameAssetDocument
        {
            Title = _gameDocument.Title,
            StartMap = _state.Document.Name,
            TitleMenu = _gameDocument.TitleMenu,
            PauseMenu = _gameDocument.PauseMenu
        };
        return document.ToGameDefinition();
    }

    private void ApplyGameDefinition(GameAssetDocument document)
    {
        try
        {
            _fileService.SaveGame(document);
        }
        catch (Exception ex) when (ex is InvalidDataException or ArgumentException)
        {
            MessageBox.Show(this, ex.Message, "Game Definition", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _gameDocument = document;
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
            SaveAllMaps();
            _shell.EnterGameMode();
            _shell.Runtime.StartGame(BuildRunDefinition(), GetAssetDirectory());
            _shell.Runtime.Focus();
        }

        _shell.Inspector.SetRunState(_shell.Runtime.IsRunning);
        UpdateStatus();
    }
}
