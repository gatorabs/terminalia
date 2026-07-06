using Terminalia.Editor.Controls.CommandBar;
using Terminalia.Editor.Controls.Inspector;

namespace Terminalia.Editor.Shell;

internal sealed class ClickOutsideFilter : IMessageFilter
{
    private const int WM_LBUTTONDOWN = 0x0201;
    private readonly InspectorPanel _inspector;
    private readonly CommandBar _commandBar;

    public ClickOutsideFilter(InspectorPanel inspector, CommandBar commandBar)
    {
        _inspector = inspector;
        _commandBar = commandBar;
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg == WM_LBUTTONDOWN && _inspector.Visible)
        {
            var cursor = Cursor.Position;
            var inspectorBounds = new Rectangle(_inspector.PointToScreen(Point.Empty), _inspector.Size);
            var commandBarBounds = new Rectangle(_commandBar.PointToScreen(Point.Empty), _commandBar.Size);
            if (!inspectorBounds.Contains(cursor) && !commandBarBounds.Contains(cursor))
            {
                _inspector.Visible = false;
            }
        }
        return false;
    }
}
