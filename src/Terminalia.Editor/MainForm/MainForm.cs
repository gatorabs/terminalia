using System.Runtime.InteropServices;
using Terminalia.Editor.Controls.Shared;
using Terminalia.Editor.Services;
using Terminalia.Editor.Styling.Colors;
using Terminalia.Editor.Styling.Cursors;
using Terminalia.Editor.Styling.Fonts;

namespace Terminalia.Editor;

public sealed partial class MainForm : Form
{
    private readonly EditorFileService _fileService = new();
    private readonly ExportService _exportService = new();
    private readonly EditorShell _shell = new();
    private readonly StartPage _startPage = new();
    private readonly LoadingOverlay _loadingOverlay = new();
    private EditorState _state = new();

    public MainForm()
    {
        Text = "Terminalia Editor";
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = EditorColors.AppBackground;
        Font = EditorFonts.UiFont;
        Cursor = EditorCursors.Terminal;
        KeyPreview = true;
        TryUseDarkTitleBar();

        _shell.Visible = false;
        Controls.Add(_shell);
        Controls.Add(_startPage);
        Controls.Add(_loadingOverlay);
        _loadingOverlay.BringToFront();
        WireEvents();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!ConfirmCanCloseAllTabs())
        {
            e.Cancel = true;
            return;
        }

        base.OnFormClosing(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (!_shell.Runtime.IsRunning)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                SaveMap();
                return true;
            }
        }

        if (_shell.Runtime.IsRunning && !IsEditingTextInput())
        {
            var keyCode = keyData & Keys.KeyCode;
            if (_shell.Runtime.TryHandleInput(keyCode))
            {
                _shell.Inspector.SetRunState(_shell.Runtime.IsRunning);
                if (!_shell.Runtime.IsRunning)
                {
                    _shell.ExitGameMode();
                }
                UpdateStatus();
                return true;
            }
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool IsEditingTextInput() =>
        ActiveControl is TextBoxBase or NumericUpDown or TrackBar;

    private void TryUseDarkTitleBar()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            return;
        }

        var value = 1;
        _ = DwmSetWindowAttribute(Handle, 20, ref value, sizeof(int));
        _ = DwmSetWindowAttribute(Handle, 19, ref value, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
}
